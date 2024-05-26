using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using ##########Mathematics;
using ##########Mathematics.Statistics;
using ##########.DXGI;

namespace ##########.Engine.Audio
{
	public class ReverbEstimator
	{
		readonly SoundSystem ss;

		public ReverbEstimator( SoundSystem ss )
		{
			this.ss	=	ss;
		}

		public ReverbParams EstimateReverbSettinngs( ReverbSimulator simulator, ReverbChannel channel, string plot )
		{
			var reverbParams	=	new ReverbParams();
			var ir				=	simulator.ImpulseResponce;

			var binCount		=	Reverberator.HistogramBinCount;
			var binWidth		=	Reverberator.T60MaxTime / binCount;

			//----------------------------------
			//	compute histograms for energy, low, mid, high bands and early and late responses :
			var binEnergyLow	= GenerateBins( ir, 0, channel, 0, 99999, binCount, binWidth, false );
			var binEnergyMid	= GenerateBins( ir, 1, channel, 0, 99999, binCount, binWidth, false );
			var binEnergyHigh	= GenerateBins( ir, 2, channel, 0, 99999, binCount, binWidth, false );

			var binDB_Low		= GenerateBins( ir, 0, channel, 0, 99999, binCount, binWidth, true );
			var binDB_Mid		= GenerateBins( ir, 1, channel, 0, 99999, binCount, binWidth, true );
			var binDB_High		= GenerateBins( ir, 2, channel, 0, 99999, binCount, binWidth, true );
								
			var binEarly		= GenerateBins( ir, 0, channel, 0,     1, binCount, binWidth, false );
			var binLate			= GenerateBins( ir, 0, channel, 1, 99999, binCount, binWidth, false );

			//----------------------------------
			//	compute T60 for low, mid and high bands :
			var T60_Low		=	CalculateT60( binDB_Low,  out var aLow,  out var bLow  );
			var T60_Mid		=	CalculateT60( binDB_Mid,  out var aMid,  out var bMid  );
			var T60_High	=	CalculateT60( binDB_High, out var aHigh, out var bHigh );

			//----------------------------------
			//	compute reverb delay :
			reverbParams.ReverbTime	=	MathUtil.Max3( T60_Low, T60_Mid, T60_High );

			//----------------------------------
			//	compute reverb wet level :
			float energyLow			=	binEnergyLow.Integral();
			float energyMid			=	binEnergyMid.Integral();
			float energyHigh		=	binEnergyHigh.Integral();
			float energyMax			=	MathUtil.Max3( energyLow, energyMid, energyHigh );
			reverbParams.WetLevel	=	SoundUtils.EnergyToDB( energyMax );

			//----------------------------------
			//	early/late parameters :
			reverbParams.EarlyDelay	=	CalculateEarlyDelay( binEarly );

			float earlyPeriod		=	binEarly.Count( b => b.Y > 0 ) * binEarly.BinSize;

			reverbParams.LateDelay	=	earlyPeriod / 2;

			var earlyDelayAmp		=	binEarly.Max( b => b.Y );
			var lateDelayAmp		=	binLate.Max( b => b.Y );
			var totalAmp			=	earlyDelayAmp + lateDelayAmp;

			reverbParams.EarlyLateMix	=	(totalAmp<=0) ? 0.5f : lateDelayAmp / totalAmp;

			//----------------------------------
			//	compute EQ parameters :
			float gainLow	=	0;
			float gainMid	=	0;
			float gainHigh	=	0;

			if (energyMax>0)
			{
				 gainLow	=	SoundUtils.EnergyToDB( energyLow  / energyMax );
				 gainMid	=	SoundUtils.EnergyToDB( energyMid  / energyMax );
				 gainHigh	=	SoundUtils.EnergyToDB( energyHigh / energyMax );
			}

			//	early reflecion round-trip in meters :
			float earlyTravelDist	=	reverbParams.EarlyDelay * 2 * 330;
			AirAbsorption.ComputeReverbAirTransmissionLoss( earlyTravelDist, out float erLow, out float erMid, out float erHigh );

			reverbParams.GainLow	=	gainLow  + erLow;
			reverbParams.GainMid	=	gainMid  + erMid;
			reverbParams.GainHigh	=	gainHigh + erHigh;

			//----------------------------------
			//	compute HF decay
			CalculateHFDecay( ref reverbParams, T60_Low, T60_Mid, T60_High );

			//----------------------------------

			if (Reverberator.ShowReverbParams)
			{
				reverbParams.Print(channel);
			}

			if (plot!=null)
			{
				reverbParams.Print(channel);

				//	https://stackoverflow.com/questions/13371449/reading-gnuplot-legend-from-csv
				//	https://stackoverflow.com/questions/16317380/column-with-empty-datapoints
				var sb = new StringBuilder();

				sb.Append("Time, ");
				sb.Append("IR Low (dB), IR Mid (dB), IR High (dB),");
				sb.Append("T60 Low, T60 Mid, T60 High,");
				sb.Append("Early Response (dB),");
				sb.Append("Late Response (dB),");
				sb.AppendLine();

				for (int i=0; i<binCount; i++)
				{
					float time = binDB_Low .Data[i].X;
					sb.Append($"{time}, ");

					sb.Append(GetCsvValue( binDB_Low , i) );
					sb.Append(GetCsvValue( binDB_Mid , i) );
					sb.Append(GetCsvValue( binDB_High, i) );

					sb.Append(GetCsvT60( time, aLow , bLow  ) );
					sb.Append(GetCsvT60( time, aMid , bMid  ) );
					sb.Append(GetCsvT60( time, aHigh, bHigh ) );

					sb.Append(GetCsvValue( binEarly, i ) );
					sb.Append(GetCsvValue( binLate, i ) );
					sb.Append(GetCsvValue( binEarly.Process( b => b.Y * NearField(b.X) ), i ) );

					sb.AppendLine();

				}

				File.WriteAllText(@"D:\git\reverb\" + plot + "Reverb.csv", sb.ToString() );
			}

			return reverbParams;
		}


		/*-----------------------------------------------------------------------------------------------
		 *	IR analysis :
		-----------------------------------------------------------------------------------------------*/

		Histogram GenerateBins( ICollection<AcousticResponse> ir, int band, ReverbChannel channel, int stepMin, int stepMax, int binCount, float binWidth, bool db )
		{
			var listener	=	ss.Listener;

			var h = Histogram.Generate( ir, binCount, binWidth, 
				r0 => ( stepMin <= r0.Step ) && ( r0.Step < stepMax ), 
				r1 => r1.Time,
				r2 => r2.Energy[band] / binWidth * listener.ReverbHRTF( channel, r2.NDir )
			);

			if (db)
			{
				h = h.Process( SoundUtils.EnergyToDB );
			}

			return h;
		}



		float NearField(float t)
		{
			var nt = Reverberator.NearTimeThreshold;
			return 1f - 1f / (1f + t*t/nt/nt);
		}


		float CalculateEarlyDelay( Histogram earlyIR )
		{
			
			switch (Reverberator.EarlyTimeMethod)
			{
				case EarlyTimeEstimationMethod.WeightedAverage:		return earlyIR.IsZero ? 0 : earlyIR.Mean();
				case EarlyTimeEstimationMethod.WeightedAverageDB:	return earlyIR.IsZero ? 0 : earlyIR.Mean( b => MathUtil.Clamp( SoundUtils.EnergyToDB(b.Y)+60, 0, 60 ) );
				case EarlyTimeEstimationMethod.NearFieldDecay:		return earlyIR.IsZero ? 0 : earlyIR.Mean( b => b.Y * NearField(b.X) );
				default: return 0;
			}
		}


		void CalculateHFDecay( ref ReverbParams reverb, float t60low, float t60mid, float t60high )
		{
			if (t60low < 0) return;

			if (t60low > t60mid && t60low > t60high )
			{
				var minT = Math.Min( t60mid, t60high );
				reverb.HFDecay = MathUtil.Clamp( minT / t60low, 0.1f, 1.0f );
			}
			else
			{
				reverb.HFDecay = 1;
			}
		}


		float CalculateT60( Histogram responseHistogram, out float A, out float B )
		{
			if (responseHistogram.IsEmpty || responseHistogram.IsZero)
			{
				A = B = 0;
				if (Reverberator.ShowReverbParams)
				{
					Log.Warning("T60 approximation failed: not enough sound energy");
				}
				return 0;
			}

			responseHistogram
				.Filter( ResponseFilter )
				.LinearApproximation( out A, out B );

			if (A==0) 
			{
				if (Reverberator.ShowReverbParams)
				{
					Log.Warning("T60 approximation failed: zero slope");
				}
				return 0;
			}

			float T60 = - (60 + B) / A;

			if (T60<0)
			{
				if (Reverberator.ShowReverbParams)
				{
					Log.Warning("T60 approximation failed: negative time");
				}
				return 0;
			}

			return T60;
		}


		bool ResponseFilter( Bucket b )
		{
			return b.Samples > SoundSystem.T60ApproxMinSampleCount && b.Y > SoundSystem.T60ApproxThresholdDB;
		}


		/*-----------------------------------------------------------------------------------------------
		 *	Plotting utils :
		-----------------------------------------------------------------------------------------------*/

		string GetCsvT60( float t, float a, float b )
		{
			float s = a * t + b;
			return (s<-60) ? "?, " : $"{s}, ";
		}


		string GetCsvValue( Histogram h, int index )
		{
			var b = h.Data[index];
			return ResponseFilter(b) ? $"{b.Y}, " : "?, ";
		}


	}
}
