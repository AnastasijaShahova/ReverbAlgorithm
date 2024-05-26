using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ##########Extensions;
using ##########Mathematics;
using ##########.Drivers.SteamAudio;
using ##########.Engine.Materials;

namespace ##########.Engine.Audio
{
	public class ReverbSimulator
	{
		Random random = new Random();

		const float FootToMeter		=	0.32f;
		const float TraceTime		=	0.3f;
		const float SpeedOfSound	=	330 / FootToMeter;
		const float TraceDistance	=	SpeedOfSound * TraceTime;

		List<AcousticResponse> ir	=	new List<AcousticResponse>();

		public ICollection<AcousticResponse>	ImpulseResponce => ir;


		int randomVectorIndex		=	0;
		const int randomVectorCount	=	0x100;
		const int randomVectorMask	=	0x0FF;


		public void SimulateImpulseResponce( SoundScene scene, Vector3 listenerPosition )
		{
			ir.Clear();
			
			int count			=	Reverberator.ReverbRaysCount;
			var randomVector	=	GetRandomVector();
			var energy			=	Color3.White * (4 * MathUtil.Pi / count);

			for (int i=0; i<count; i++)
			{
				var dir	=	Hammersley.SphereUniform( i, count ) * TraceDistance;
					dir	=	Vector3.Reflect( dir, randomVector );

				TracePhonon( scene, ir, 0, listenerPosition, 0, listenerPosition, dir, energy );
			}
		}



		Vector3 GetRandomVector()
		{	
			return Hammersley.SphereUniform( (randomVectorIndex++) & randomVectorMask, randomVectorCount );
		}


		void TracePhonon( SoundScene scene, List<AcousticResponse> ir, int step, Vector3 listenerPos, float baseTime, Vector3 origin, Vector3 direction, Color3 energy )
		{
			if (step>Reverberator.ReverbMaxBounces) return;
			if (scene==null) return;

			float eps = 1/8192f;

			if ( scene.Intersect( origin, origin + direction, -eps, out var fraction, out var hitPoint, out var hitNormal, out var color ))
			{
				if (Reverberator.AcousticMaterialOverride!=AcousticMaterial.Default)
				{
					color = AcousticMaterialSettings.GetMaterial( Reverberator.AcousticMaterialOverride ).GetAcousticColor();
				}

				var dirNorm		= direction.Normalized();
				hitNormal.Normalize();
				var travelDist	= Vector3.Distance( origin, hitPoint );
				var travelTime	= travelDist / SpeedOfSound;  // sound travel time
				var view		= listenerPos - hitPoint;
				var viewDist	= view.Length();
				var viewNorm	= view / viewDist;
				var timeStamp	= viewDist / SpeedOfSound + baseTime + travelTime;

				var e	=	energy * color;

				//AirAbsorption.AttenuateSoundEnergy( dist, ref e );

				var lambert	=	Math.Max( Vector3.Dot(dirNorm,hitNormal), 0 );
				var hvector	=	(dirNorm + viewNorm).Normalized();

				var phong	=	Math.Abs( Vector3.Dot( hitNormal, hvector ) );
					phong	=	(float)Math.Pow( phong, 8 ) * ( 8 + 1 ) / 2 / MathUtil.Pi;

				var falloff =	1f;

				var	factor	=	MathUtil.Lerp( phong, lambert, Reverberator.DiffuseFactorHearing );
				var k		=	Reverberator.ResponseFalloffScale;

				if (Reverberator.ResponseFalloff==Falloff.InverseQuadraticSmooth)
				{
					falloff = 1f / (1f + viewDist*viewDist/k/k);
				}

				if (!Reverberator.OccludeResponse || !scene.IsOccluded( listenerPos, hitPoint))
				{
					var dir			=	hitPoint - listenerPos;
					var hearEnergy	=	e * factor * falloff;
					//AirAbsorption.AttenuateSoundEnergy( dir.Length(), ref hearEnergy );
					ir.Add( new AcousticResponse( step, timeStamp, hearEnergy, hitPoint, dir ) );
				}

				Vector3 reflectVector;
				Vector3 vectorOnSphere = Hammersley.SphereUniform( random.Next(0,179), 179 );

				if (random.NextFloat(0,1) > Reverberator.DiffuseFactorTracing)
				{
					reflectVector = vectorOnSphere * 0.1f + Vector3.Reflect( direction, hitNormal );
				}
				else
				{
					reflectVector = vectorOnSphere * 0.5f + hitNormal;
				}

				reflectVector.Normalize();
				reflectVector *= TraceDistance;

				TracePhonon( scene, ir, step+1, listenerPos, baseTime + travelTime, hitPoint, reflectVector, e );
			}
		}

	}
}
