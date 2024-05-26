using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ##########Mathematics;

namespace ##########.Engine.Audio
{
	public static class SoundUtils
	{
		public static float EnergyToDB( float energy )
		{
			const float minE = 1e-12f;
			return 10 * (float)Math.Log10( Math.Max( minE, energy ) );
		}


		public static float DecibelsToEnergy( float db )
		{
			return (float)Math.Pow( 10, db / 10 );
		}


		public static float PerceptionalScale( float scale )
		{
			scale			=	MathUtil.Clamp( scale, 0, 1 );
			var decibels	=	60 * scale - 60;
			var energy		=	MathUtil.Exp10( decibels / 10 );

			return energy;
		}
	}
}
