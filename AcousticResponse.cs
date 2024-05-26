using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ##########Mathematics;

namespace ##########.Engine.Audio
{
	public struct AcousticResponse
	{
		public int		Step;
		public float	Time;
		public Color3	Energy;
		public Vector3	Location;
		public Vector3	NDir;

		public AcousticResponse( int step, float time, Color3 energy, Vector3 location, Vector3 dir )
		{
			Step		=	step;
			Time		=	time; 
			Energy		=	energy;
			Location	=	location;
			NDir		=	Vector3.Normalize( dir );
		}

		public override string ToString()
		{
			return $"{Time} {Energy}";
		}
	}
}
