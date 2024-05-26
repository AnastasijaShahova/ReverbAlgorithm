using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ##########Mathematics;

namespace ##########.Engine.Audio
{
	/// <summary>
	/// http://resource.npl.co.uk/acoustics/techguides/absorption/
	/// </summary>
	public static class AirAbsorption
	{
		/*-----------------------------------------------------------------
		 * 	Air absorption coefficients (dB/m) for following conditions:
		 * 		Temperature:	25 degrees Celsius
		 * 		Pressure:		101 kPa
		 * 		Humidity:		75%
		-----------------------------------------------------------------*/

		const float Absorption_62Hz		=	0.00007f;
		const float Absorption_125Hz	=	0.00027f;
		const float Absorption_250Hz	=	0.00100f;
		const float Absorption_500Hz	=	0.00301f;
		const float Absorption_1000Hz	=	0.00628f;
		const float Absorption_2000Hz	=	0.01055f;
		const float Absorption_4000Hz	=	0.02157f;
		const float Absorption_8000Hz	=	0.06316f;

		/// <summary>
		/// Computed ait absorption for three frequency bands: 250 Hz, 1000 Hz and 4000 Hz
		/// Crossfade is on 500 Hz and 2000 Hz
		/// Result: EQ3 paramters for each band.
		/// </summary>
		/// <param name="distance">Distance in meters</param>
		/// <param name="low"></param>
		/// <param name="mid"></param>
		/// <param name="high"></param>
		public static void ComputeAirTransmissionLoss( float distance, out float low, out float mid, out float high )
		{
			float scale	=	SoundSystem.AirAbsorptionScale;
			low			=	MathUtil.Clamp( - distance * scale * Absorption_250Hz  , -80, 0 );
			mid			=	MathUtil.Clamp( - distance * scale * Absorption_1000Hz , -80, 0 );
			high		=	MathUtil.Clamp( - distance * scale * Absorption_4000Hz , -80, 0 );
		}



		public static void ComputeReverbAirTransmissionLoss( float distance, out float low, out float mid, out float high )
		{
			float scale	=	Reverberator.AirAbsorptionScale;
			low			=	MathUtil.Clamp( - distance * scale * Absorption_62Hz  ,  -80, 0 );
			mid			=	MathUtil.Clamp( - distance * scale * Absorption_500Hz ,  -80, 0 );
			high		=	MathUtil.Clamp( - distance * scale * Absorption_4000Hz , -80, 0 );
		}



		/// <summary>
		/// Computed ait absorption for three frequency bands used by reverberator
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="soundEnergy"></param>
		public static void AttenuateSoundEnergy( float distance, ref Color3 soundEnergy )
		{
			float scale		=	Reverberator.AirAbsorptionScale;
			float lowDB		=	MathUtil.Clamp( - distance * scale * Absorption_62Hz   , -80, 0 );
			float midDB		=	MathUtil.Clamp( - distance * scale * Absorption_500Hz  , -80, 0 );
			float highDB	=	MathUtil.Clamp( - distance * scale * Absorption_4000Hz , -80, 0 );

			float low		=	SoundUtils.DecibelsToEnergy( lowDB );
			float mid		=	SoundUtils.DecibelsToEnergy( midDB );
			float high		=	SoundUtils.DecibelsToEnergy( highDB );

			soundEnergy.Red		*=	low;
			soundEnergy.Green	*=	mid;
			soundEnergy.Blue	*=	high;
		}
	}
}
