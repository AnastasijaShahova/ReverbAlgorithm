using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FMOD;
using ##########.Build;
using ##########;
using ##########Extensions;
using ##########Mathematics;
using ##########Mathematics.FFT;
using ##########Mathematics.Statistics;
using ##########.Engine.Audio;
using ##########.Engine.Graphics.Scenes;
using Native.Embree;
using ##########.Direct3D11;
using System.Collections.Concurrent;
using System.Threading;
using ##########.Engine.Materials;

namespace ##########.Engine.Audio 
{
	public class SoundScene : DisposableBase
	{
		//	scene is shared between main and reverb computing thread
		//	we need to synchronize RTC scene (yes, we need)
		//	NOTE : Intersects are thread safe, scene changes could be a problem
		readonly ReaderWriterLock rwLock = new ReaderWriterLock();

		internal RtcScene RtcScene => rtcScene;

		Rtc			rtc;
		RtcScene	rtcScene;

		readonly Dictionary<uint,Color3> materialMapping = new Dictionary<uint, Color3>();

		bool hasUncommittedChanges = true;


		public SoundScene()
		{
			var sceneFlags	=	SceneFlags.Static|SceneFlags.Dynamic|SceneFlags.Coherent;
			var algFlags	=	AlgorithmFlags.Intersect1;

			rtc				=	new Rtc();
			rtcScene		=	new RtcScene( rtc, sceneFlags, algFlags );
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				SafeDispose( ref rtcScene );
				SafeDispose( ref rtc );
			}

			base.Dispose(disposing);
		}


		public SoundMesh AddStaticMesh( Mesh sourceMesh, Matrix transform, AcousticMaterial material )
		{
			rwLock.AcquireWriterLock(-1);

			try 
			{
				var indices		=	sourceMesh.GetIndices();
				var vertices	=	sourceMesh.Vertices
									.Select( v1 => Vector3.TransformCoordinate( v1.Position, transform ) )
									.Select( v2 => new Vector4( v2.X, v2.Y, v2.Z, 0 ) )
									.ToArray();

				var id			=	rtcScene.NewTriangleMesh( GeometryFlags.Dynamic, indices.Length/3, vertices.Length );

				var pVerts		=	rtcScene.MapBuffer( id, BufferType.VertexBuffer );
				var pInds		=	rtcScene.MapBuffer( id, BufferType.IndexBuffer );

				##########.Utilities.Write( pVerts, vertices, 0, vertices.Length );
				##########.Utilities.Write( pInds,  indices,  0, indices.Length );

				rtcScene.UnmapBuffer( id, BufferType.VertexBuffer );
				rtcScene.UnmapBuffer( id, BufferType.IndexBuffer );

				var color  = AcousticMaterialSettings.GetMaterial( material ).GetAcousticColor();

				if (!materialMapping.ContainsKey(id))
				{
					materialMapping.Add( id, color );
				}
				else
				{
					Log.Warning($"SoundScene : material for mesh {id} is already mapped");
				}

				var mesh = new SoundMesh( id );

				hasUncommittedChanges = true;

				return mesh;
			}
			finally
			{
				rwLock.ReleaseWriterLock();
			}
		}


		public void RemoveMesh( SoundMesh mesh )
		{
			rwLock.AcquireWriterLock(-1);

			try
			{
				materialMapping.Remove( mesh.ID );
				rtcScene.DeleteGeometry( mesh.ID );
				hasUncommittedChanges = true;
			}
			finally
			{
				rwLock.ReleaseWriterLock();
			}
		}


		public void CommitChanges()
		{
			rwLock.AcquireWriterLock(-1);

			try
			{
				rtcScene.Commit();
				hasUncommittedChanges = false;
			}
			finally
			{
				rwLock.ReleaseWriterLock();
			}
		}


		/*-----------------------------------------------------------------------------------------------
		 *	Basic tracing :
		-----------------------------------------------------------------------------------------------*/

		public bool IsOccluded( Vector3 pointA, Vector3 pointB )
		{
			rwLock.AcquireReaderLock(-1);

			try
			{
				if (hasUncommittedChanges)
				{
					return false;
				}

				var dir = pointB - pointA;

				return rtcScene.Occluded( pointA.X, pointA.Y, pointA.Z, dir.X, dir.Y, dir.Z, 0, 1 );
			}
			finally
			{
				rwLock.ReleaseReaderLock();
			}
		}


		public bool Intersect( Vector3 start, Vector3 end, float eps, out float fraction, out Vector3 hit, out Vector3 normal, out Color3 color )
		{
			rwLock.AcquireReaderLock(-1);

			try
			{
				if (hasUncommittedChanges)
				{
					fraction = 0;
					hit = Vector3.Zero;
					normal = Vector3.Zero;
					color = Color3.Black;
					return false;
				}

				var dir = end - start;

				RtcRay ray		=	new RtcRay();
				ray.Direction	=	new RtcVector3 { X = dir.X, Y = dir.Y, Z = dir.Z };
				ray.Origin		=	new RtcVector3 { X = start.X, Y = start.Y, Z = start.Z };
				ray.TNear		=	0;
				ray.TFar		=	1;
				ray.Mask		=	0xFFFFFFFF;
				ray.GeometryId	=	RtcRay.InvalidGeometryID;
				ray.PrimitiveId	=	RtcRay.InvalidGeometryID;
				ray.InstanceId	=	RtcRay.InvalidGeometryID;

				var anyHit		=	rtcScene.Intersect( ref ray );

				color			=	Color3.Black;

				if (anyHit)
				{
					fraction	=	ray.TFar;
					hit			=	Vector3.Lerp( start, end, fraction + eps );
					normal		=	new Vector3( ray.HitNormal.X, ray.HitNormal.Y, ray.HitNormal.Z );

					if (!materialMapping.TryGetValue( ray.GeometryId, out color ))
					{
						color		=	Color3.Black;
					}

					return true;
				}
				else
				{
					fraction	=	1;
					hit			=	end;
					normal		=	Vector3.Zero;
					return false;
				}
			}
			finally
			{
				rwLock.ReleaseReaderLock();
			}
		}


		public void ApplyWallTL( Vector3 sourcePos, Vector3 listenerPos, int numSteps, ref float low, ref float mid, ref float high )
		{
			float eps = 1f/16536f;

			for (int i=0; i<numSteps; i++)
			{
				var isOccluded = Intersect( sourcePos, listenerPos, eps, out var _, out var hitPos, out var normal, out _ );

				var dir		=	listenerPos - sourcePos;
				var dotSign	=	Vector3.Dot( normal, dir );

				if (isOccluded)
				{
					if (dotSign<0)
					{
						low		-=	SoundSystem.TransmissionLossScale * AcousticMaterialSettings.TL_250Hz;
						mid		-=	SoundSystem.TransmissionLossScale * AcousticMaterialSettings.TL_1000Hz;
						high	-=	SoundSystem.TransmissionLossScale * AcousticMaterialSettings.TL_4000Hz;
					}

					sourcePos = hitPos;
				}
				else
				{
					return;
				}
			}
		}
	}
}
