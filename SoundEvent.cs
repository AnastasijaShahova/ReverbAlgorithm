using System;
using ##########;
using ##########Mathematics;
using FMOD;
using FMOD.Studio;
using System.Runtime.InteropServices;

namespace ##########.Engine.Audio
{
	public sealed class SoundEvent
	{
		readonly SoundSystem ss;
		readonly FMOD.Studio.System system;
		readonly EventDescription eventDesc;
		readonly string path;

		public string Path 
		{
			get { return path; }
		}

		EVENT_CALLBACK callback;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="voice"></param>
        internal SoundEvent( SoundSystem ss, EventDescription eventDesc )
        {
			this.ss			=	ss;
			this.eventDesc	=	eventDesc;
			this.system		=	ss.system;

			FmodExt.ERRCHECK( eventDesc.getPath( out path ) );

			//callback	=	TestCallback;
			//GCHandle.Alloc(callback);
			//eventDesc.setCallback( callback );
		}


		RESULT TestCallback(EVENT_CALLBACK_TYPE type, IntPtr _event, IntPtr parameters)
		{
			Log.Debug($"fmod callback: {type}");
			return RESULT.OK;
		}


		/// <summary>
		/// Creates instance of the given event
		/// </summary>
		/// <returns></returns>
		public SoundEventInstance CreateInstance ()
		{
			return new SoundEventInstance( ss, eventDesc );
		}


		public float MaximumDistance 
		{
			get 
			{
				FmodExt.ERRCHECK( eventDesc.getMinMaxDistance( out float _, out float max ) );
				return max;
			}
		}


		public float MinimumDistance 
		{
			get 
			{
				FmodExt.ERRCHECK( eventDesc.getMinMaxDistance( out float min, out float _ ) );
				return min;
			}
		}
	}
}
