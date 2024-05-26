using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ##########Mathematics;
using ##########.XAPO.Fx;

namespace ##########.Engine.Audio
{
	public class Listener
	{
		public readonly Vector3 Position;
		public readonly Vector3 Velocity;
		public readonly Vector3 Up;
		public readonly Vector3 Forward;
		public readonly Vector3 Right;

		readonly Vector3	leftEarDirection;
		readonly Vector3	rightEarDirection;


		public Listener()
		{
			Position	=	Vector3.Zero;
			Velocity	=	Vector3.Zero;
			Forward		=	Vector3.ForwardRH;
			Up			=	Vector3.Up;
			Right		=	Vector3.Right;
		}


		public Listener( Vector3 position, Vector3 forward, Vector3 up, Vector3 velocity )
		{
			Position	=	position;
			Velocity	=	velocity;
			Forward		=	Vector3.Normalize( forward );
			Up			=	Vector3.Normalize( up );
			Right		=	Vector3.Normalize( Vector3.Cross( forward, up ) );

			var sqrt2	=	(float)Math.Sqrt(2);

			leftEarDirection	=	Vector3.Normalize( Forward * sqrt2 - Right * sqrt2 );
			rightEarDirection	=	Vector3.Normalize( Forward * sqrt2 + Right * sqrt2 );
		}


		/// <summary>
		/// Very rough approximation of HRTF for reverberation
		/// https://www.desmos.com/calculator/gptzotaioz
		/// </summary>
		public float ReverbHRTF( ReverbChannel channel, Vector3 directionToSound )
		{
			var axis	=	channel == ReverbChannel.Left ? -Right : Right;
			var dot		=	Vector3.Dot( axis, directionToSound );

			//	reduce floor reflections, since they give too much early response
			//	increase energy from upper hemisphere and decrease from lower hemisphere :
			var ground	=	directionToSound.Y > 0 ? 1 + Reverberator.FloorCeilingBalance : 1 - Reverberator.FloorCeilingBalance;

			return dot > 0 ? ground : 0;
		}
	}
}
