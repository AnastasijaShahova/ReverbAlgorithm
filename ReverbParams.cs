using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMOD;
using ##########Mathematics;

namespace ##########.Engine.Audio
{
	public enum ReverbChannel
	{
		Left,
		Right,
	}

	public class ReverbParams
	{
		public float ReverbTime		;
		public float WetLevel		;
		public float EarlyDelay		;
		public float LateDelay		;
		public float EarlyLateMix	;
		public float HFDecay		;

		public float GainLow		;
		public float GainMid		;
		public float GainHigh		;


		public ReverbParams()
		{
			Reset();
		}


		public void Reset()
		{
			ReverbTime		=	0.1f;
			WetLevel		=	-60;
			EarlyDelay		=	0;
			LateDelay		=	0;
			EarlyLateMix	=	0.5f;
			HFDecay			=	1.0f;

			GainLow			=	0;
			GainMid			=	0;
			GainHigh		=	0;
		}



		/// <summary>
		/// Seek reverb parameters
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="target">Could be NULL</param>
		public void Seek( float dt, ReverbParams target )
		{
			if (target!=null)
			{
				ReverbTime		=	MathUtil.Seek( ReverbTime	, target.ReverbTime		,  2.0f * dt );
				WetLevel		=	MathUtil.Seek( WetLevel		, target.WetLevel		, 30.0f * dt );
				EarlyDelay		=	MathUtil.Seek( EarlyDelay	, target.EarlyDelay		,  0.5f * dt );
				LateDelay		=	MathUtil.Seek( LateDelay	, target.LateDelay		,  0.5f * dt );
				EarlyLateMix	=	MathUtil.Seek( EarlyLateMix	, target.EarlyLateMix	,  0.5f * dt );
				HFDecay			=	MathUtil.Seek( HFDecay		, target.HFDecay		,  0.5f * dt );

				GainLow			=	MathUtil.Seek( GainLow		, target.GainLow		, 30.0f * dt );
				GainMid			=	MathUtil.Seek( GainMid		, target.GainMid		, 30.0f * dt );
				GainHigh		=	MathUtil.Seek( GainHigh		, target.GainHigh		, 30.0f * dt );
			}
		}


		public void Apply(FMOD.Studio.System system, ReverbChannel channel, string busPath )
		{
			string prefix = "";

			switch (channel)
			{
				case ReverbChannel.Left:	prefix = "L_"; 	break;
				case ReverbChannel.Right:	prefix = "R_";	break;
			}

			//	FmodExt.ERRCHECK( system.setParameterByName( prefix + "ReverbTime"			, ReverbTime	) );
			//	FmodExt.ERRCHECK( system.setParameterByName( prefix + "ReverbWetLevel"		, WetLevel		) );
			//	FmodExt.ERRCHECK( system.setParameterByName( prefix + "ReverbEarlyDelay"	, EarlyDelay	) );
			//	FmodExt.ERRCHECK( system.setParameterByName( prefix + "ReverbLateDelay"		, LateDelay		) );
			//	FmodExt.ERRCHECK( system.setParameterByName( prefix + "ReverbEarlyLateMix"	, EarlyLateMix	) );
			//	FmodExt.ERRCHECK( system.setParameterByName( prefix + "ReverbHFDecay"		, HFDecay		) );
			//	FmodExt.ERRCHECK( system.setParameterByName( prefix + "ReverbGainLow"		, GainLow		) );
			//	FmodExt.ERRCHECK( system.setParameterByName( prefix + "ReverbGainMid"		, GainMid		) );
			//	FmodExt.ERRCHECK( system.setParameterByName( prefix + "ReverbGainHigh"		, GainHigh		) );

			if (TryGetDSP(system, busPath, DSP_TYPE.THREE_EQ, out var dspEQ3))
			{
				FmodExt.ERRCHECK( dspEQ3.setParameterFloat( (int)DSP_THREE_EQ.LOWGAIN,  GainLow  ) );
				FmodExt.ERRCHECK( dspEQ3.setParameterFloat( (int)DSP_THREE_EQ.MIDGAIN,  GainMid  ) );
				FmodExt.ERRCHECK( dspEQ3.setParameterFloat( (int)DSP_THREE_EQ.HIGHGAIN, GainHigh ) );

				FmodExt.ERRCHECK( dspEQ3.setParameterInt( (int)DSP_THREE_EQ.CROSSOVERSLOPE, 1	 ) );

				FmodExt.ERRCHECK( dspEQ3.setParameterFloat( (int)DSP_THREE_EQ.LOWCROSSOVER,   200f ) );
				FmodExt.ERRCHECK( dspEQ3.setParameterFloat( (int)DSP_THREE_EQ.HIGHCROSSOVER, 1500f ) );
			}

			if (TryGetDSP(system, busPath, DSP_TYPE.SFXREVERB, out var dspReverb))
			{
				//	https://www.fmod.com/docs/2.00/api/effects-reference.html#sfx-reverb
				//	https://www.fmod.com/docs/2.00/api/core-api-common-dsp-effects.html#fmod_dsp_sfxreverb
				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.DECAYTIME			, 1000 * ReverbTime					) );
				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.EARLYDELAY		, 1000 * EarlyDelay					) );
				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.LATEDELAY			, 1000 * LateDelay					) );

				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.HFREFERENCE		, AcousticMaterialSettings.GetFrequency(4)	) );
				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.HFDECAYRATIO		, 100 * HFDecay						) );

				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.DIF##########			, 100								) );
				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.DENSITY			, 100								) );
				
				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.LOWSHELFFREQUENCY	, 250								) );
				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.LOWSHELFGAIN		, 0									) );
				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.HIGHCUT			, Reverberator.ReverbHighCut		) );

				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.EARLYLATEMIX		, 100 * EarlyLateMix				) );

				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.WETLEVEL			, WetLevel							) );
				FmodExt.ERRCHECK( dspReverb.setParameterFloat( (int)DSP_SFXREVERB.DRYLEVEL			, -80								) );

				FmodExt.ERRCHECK( dspReverb.setActive( true ) );
			}
		}


		bool TryGetDSP(FMOD.Studio.System system, string busPath, DSP_TYPE requestedDspType, out DSP dsp)
		{
			dsp = new DSP();

			FmodExt.ERRCHECK( system.getBus( busPath, out var bus ) );

			if ( bus.getChannelGroup( out var cg )==RESULT.OK )
			{
				if ( cg.getNumDSPs( out int numDSPs )==RESULT.OK )
				{
					for (int i=0; i<numDSPs; i++)
					{
						cg.getDSP(i, out dsp);

						FmodExt.ERRCHECK( dsp.getType(out var dspType));

						if (dspType==requestedDspType)
						{
							return true;
						}
					}
				}
			}

			return false;
		}


		public void Print(ReverbChannel ch)
		{
			Log.Message($"REVERB {ch,5}: Wet = {WetLevel,4:0}dB, T60 = {ReverbTime,4:0.00}s, Early={EarlyDelay,5:0.000}s, Late={LateDelay,5:0.000}s, E/L={EarlyLateMix*100,3:0}%, HFD={HFDecay*100,3:0}% EQ3 {GainLow,3:0} {GainMid,3:0} {GainHigh,3:0}");
		}
	}
}
