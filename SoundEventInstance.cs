using System;
using ##########.XAudio2;
using FMOD;
using FMOD.Studio;
using ##########;
using ##########Mathematics;
using ##########.Engine.Graphics.Utils;
using ##########.Drivers.SteamAudio;
using System.Runtime.InteropServices;

namespace ##########.Engine.Audio
{
	public sealed class SoundEventInstance
	{
		readonly SoundSystem ss;

		readonly EventDescription desc;
		readonly EventInstance inst;
		readonly FMOD.Studio.System system;
		readonly bool is3D;

		readonly GCHandle userDataHandle;

		int updateCounter = 0;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="voice"></param>
		internal SoundEventInstance( SoundSystem ss, EventDescription desc )
		{
			this.ss		=	ss;
			this.desc	=	desc;
			this.system	=	ss.system;

			desc.is3D(out is3D);

			FmodExt.ERRCHECK( desc.createInstance( out inst ) );

			inst.setReverbLevel(0,1);
		}


		public override string ToString()
		{
			string path;
			desc.getPath( out path );
			return string.Format("[{0}]", path);
		}


		public void ListDSPs()
		{
			FmodExt.ERRCHECK( inst.getChannelGroup( out var cg ) );
			FmodExt.ERRCHECK( cg.getNumDSPs( out var numDSPs ) );

			for (int i=0; i<numDSPs; i++)
			{
				cg.getDSP( i, out var dsp );

				dsp.getInfo( out string name, out var version, out var channels, out int cfgW, out int cfgH );
				Log.Debug($"DSP[{i}] - {name} v:{version} ch:{channels}");
			}
		}


		public void SetParameter(string name, float value, bool ignoreSeekSpeed)
		{
			inst.setParameterByName( name, value, ignoreSeekSpeed );
		}


		/// <summary>
		/// 
		/// </summary>
		public void Release()
        {
			FmodExt.ERRCHECK( inst.release() );
		}


		public void Start ()
		{
			FmodExt.ERRCHECK( inst.start() );
		}


		public void Stop ( bool immediate )
		{
			var mode = immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT;
			FmodExt.ERRCHECK( inst.stop( mode ) );
		}



		public void Set3DParameters ( Vector3 position, Vector3 forward, Vector3 up, Vector3 velocity )
		{
			ATTRIBUTES_3D attrs;

			float dt = 1f / 60f;

			attrs.forward	=	FmodExt.Convert( forward	);
			attrs.position	=	FmodExt.Convert( position	);
			attrs.up		=	FmodExt.Convert( up			);
			attrs.velocity	=	FmodExt.Convert( velocity	);

			FmodExt.ERRCHECK( inst.set3DAttributes( attrs ) );

			if (is3D)
			{
				var instant		=	updateCounter==0;
				updateCounter++;

				//	compute air absorption :
				var listenerPosition	=	ss.ListenerPosition;
				var soundPosition		=	position;
				var distance			=	Vector3.Distance( soundPosition, listenerPosition );

				//	compute air TL :
				AirAbsorption.ComputeAirTransmissionLoss( distance, out var tlLow, out var tlMid, out var tlHigh );

				//	compute occlusion :
				ss.Scene?.ApplyWallTL( soundPosition, listenerPosition, 8, ref tlLow, ref tlMid, ref tlHigh );

				inst.setParameterByName( "TLLow" , MathUtil.Clamp( tlLow , -80, 0 ), instant );
				inst.setParameterByName( "TLMid" , MathUtil.Clamp( tlMid , -80, 0 ), instant );
				inst.setParameterByName( "TLHigh", MathUtil.Clamp( tlHigh, -80, 0 ), instant );

			}
		}



		public void Set3DParameters ( Vector3 position )
		{
			Set3DParameters( position, Vector3.ForwardRH, Vector3.Up, Vector3.Zero );
		}


		public void Set3DParameters ( Vector3 position, Vector3 velocity )
		{
			Set3DParameters( position, Vector3.ForwardRH, Vector3.Up, velocity );
		}



		PLAYBACK_STATE GetPlaybackState()
		{
			PLAYBACK_STATE result;
			FmodExt.ERRCHECK( inst.getPlaybackState( out result ) );
			return result;
		}


		public bool IsStopped => GetPlaybackState() == PLAYBACK_STATE.STOPPED;


		public float ReverbLevel 
		{
			set 
			{
				FmodExt.ERRCHECK( inst.setReverbLevel( 0, value ) );
			}
			get 
			{
				float result;
				FmodExt.ERRCHECK( inst.getReverbLevel( 0, out result ) );
				return result;
			}
		}
	}
}
