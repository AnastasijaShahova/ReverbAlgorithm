using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using ##########;
using ##########;
using ##########.Engine.Common;
using ##########Configuration;
using FMOD;
using FMOD.Studio;
using ##########Input;
using ##########Mathematics;
using ##########Content;
using ##########Extensions;
using ##########.Drivers.SteamAudio;
using ##########.MediaFoundation.DirectX;

namespace ##########.Engine.Audio 
{
	[##########(typeof(SoundBank))]
	public sealed class SoundBankLoader : ##########
	{
		public override object Load( ContentManager content, Stream stream, Type requestedType, string assetPath, IStorage storage )
		{
			return new SoundBank( content.Game.SoundSystem, stream.ReadAllBytes() );
		}
	}

	public sealed partial class SoundSystem : GameComponent 
	{
		internal FMOD.Studio.System system;
		internal FMOD.System		lowlevel;

		const string eventPrefix = "event:/";
		const string busPrefix = "bus:/";


		public SoundScene Scene { get; set; }
		public Reverberator Reverb => reverb;
		Reverberator reverb;


		/// <summary>
		/// 
		/// </summary>
		public SoundSystem ( Game game ) : base(game)
		{
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize()
		{
			var studioFlags		=	FMOD.Studio.INITFLAGS.NORMAL | FMOD.Studio.INITFLAGS.LIVEUPDATE;
			var lowlevelFlags	=	FMOD.INITFLAGS.NORMAL | FMOD.INITFLAGS._3D_RIGHTHANDED;
			var speakerMode		=	FMOD.SPEAKERMODE.DEFAULT;

			FmodExt.ERRCHECK( FMOD.Studio.System.create( out system ) );
			FmodExt.ERRCHECK( system.getCoreSystem( out lowlevel ) );
			FmodExt.ERRCHECK( lowlevel.setSoftwareFormat( 0, speakerMode, 0 ) );
			FmodExt.ERRCHECK( system.initialize( 1024, studioFlags, lowlevelFlags, IntPtr.Zero ) );

			lowlevel.set3DSettings( 1, 3.28f, 1 );

			FmodExt.ERRCHECK( lowlevel.loadPlugin("resonanceaudio", out _ ) );

			Game.Invoker.RegisterCommand("soundInfo", ()=> new SoundInfoCommand(system) );
			Game.Invoker.RegisterCommand("mesureReverb", ()=> new MeasureReverb() );

			//	create effect controllers :
			reverb	=	new Reverberator(this);
		}


		public SoundBank LoadSoundBank ( ContentManager content, string path )
		{
			return content.Load<SoundBank>( path );
		}


		public IEnumerable<string> GetEventNames()
		{
			var list = new List<string>();

			FmodExt.ERRCHECK( system.getBankList(out var bankList) );

			foreach (var bank in bankList)
			{
				FmodExt.ERRCHECK( bank.getEventList( out var eventDescs ) );

				for ( int i=0; i<eventDescs.Length; i++ ) 
				{
					list.Add( eventDescs[i].GetPath().Replace(eventPrefix,"") );
				}
			}

			return list;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) 
			{
				FmodExt.ERRCHECK( system.release() );
			}
		}


		void SetBusVolume(string path, float volume)
		{
			FmodExt.ERRCHECK(system.getBus(path, out Bus master));

			master.setVolume(volume);
		}


		/*-----------------------------------------------------------------------------------------
		 *	Spatial Sound Stuff
		-----------------------------------------------------------------------------------------*/

		public const string BusMaster				=	"bus:/"						;
		public const string BusMusic				=	"bus:/Music"				;
		public const string BusUI					=	"bus:/UI"					;
		public const string BusSFX					=	"bus:/SFX"					;
		public const string BusSFXReverbLeft		=	"bus:/SFX/ReverbLeft"		;
		public const string BusSFXReverbRight		=	"bus:/SFX/ReverbRight"		;


		/// <summary>
		/// Updates sound.
		/// </summary>			
		public override void Update( GameTime gameTime )
		{
			SetBusVolume( BusMaster	, MasterVolume );
			SetBusVolume( BusMusic	, MusicVolume );
			SetBusVolume( BusUI		, UIVolume );
			SetBusVolume( BusSFX	, SFXVolume );

			Reverb.Update( gameTime );

			FmodExt.ERRCHECK( system.update() );
		}


		/*-----------------------------------------------------------------------------------------
		 *	3D sound stuff :
		-----------------------------------------------------------------------------------------*/

		public Vector3 ListenerPosition { get; private set; }

		public Listener Listener { get; private set; } = new Listener();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="forward"></param>
		/// <param name="up"></param>
		public void SetListener ( Vector3 position, Vector3 forward, Vector3 up, Vector3 velocity )
		{
			ATTRIBUTES_3D attrs;
			
			ListenerPosition	=	position;

			Listener		=	new Listener( position, forward, up, velocity );

			attrs.forward	=	FmodExt.Convert( forward	);
			attrs.position	=	FmodExt.Convert( position	);
			attrs.up		=	FmodExt.Convert( up			);
			attrs.velocity	=	FmodExt.Convert( velocity	);

			FmodExt.ERRCHECK( system.setListenerAttributes( 0, attrs ) );
		}



		/// <summary>
		/// Gets event by name
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public SoundEvent GetEvent ( string path )
		{
			EventDescription eventDesc;
			string eventPath = Path.Combine( eventPrefix, path);

			var result = system.getEvent( eventPath, out eventDesc );

			if (result!=FMOD.RESULT.OK) 
			{
				throw new SoundException( result, eventPath );
			}

			return new SoundEvent( this, eventDesc );
		}


		public SoundDescriptor GetEventDescriptor( string path )
		{
			EventDescription eventDesc;
			string eventPath = Path.Combine( eventPrefix, path);

			var result = system.getEvent( eventPath, out eventDesc );

			if (result!=FMOD.RESULT.OK) 
			{
				return null;
			}

			var desc = new SoundDescriptor();
			eventDesc.getLength( out desc.LengthMSec );
			eventDesc.getMinMaxDistance( out desc.MinDistance, out desc.MaxDistance );
			return desc;
		}


		[Obsolete]
		public float GetEventLength( string path )
		{
			var eventDesc = GetEventDescriptor(path);

			if (eventDesc!=null)
			{
				return GetEventDescriptor(path).LengthMSec / 1000.0f;
			}
			
			Log.Warning($"Sound event '{path}' not found!");
			return 0;
		}


		public int GetSoundLength( string path )
		{
			var eventDesc = GetEventDescriptor(path);

			if (eventDesc!=null)
			{
				return GetEventDescriptor(path).LengthMSec;
			}
			
			Log.Warning($"Sound event '{path}' not found!");
			return 0;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool Play2DEvent ( string path )
		{
			EventDescription desc;
			EventInstance inst;
			var eventPath = Path.Combine(eventPrefix, path);

			if (FmodExt.Failed( system.getEvent(eventPath, out desc) ))
			{
				Log.Warning("Failed to play event: {0}", eventPath );
				return false;
			}

			bool is3d;
			FmodExt.ERRCHECK( desc.is3D( out is3d ) );

			if (is3d) 
			{
				Log.Warning("Event '{0}' is 3D", eventPath);
			}

			if (FmodExt.Failed( desc.createInstance( out inst ) ))
			{
				return false;
			}

			inst.start();
			inst.release();

			return true;
		}
	}

}
