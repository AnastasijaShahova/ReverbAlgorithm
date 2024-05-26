using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FMOD;
using ##########;
using ##########Shell;

namespace ##########.Drivers.SteamAudio
{
	public class SoundInfoCommand : ICommand
	{
		readonly FMOD.Studio.System system;

		[CommandLineParser.Option]
		[CommandLineParser.Name("bus")]
		public bool ShowBuses { get; set; }

		[CommandLineParser.Option]
		[CommandLineParser.Name("event")]
		public bool ShowEvents { get; set; }

		[CommandLineParser.Option]
		[CommandLineParser.Name("vca")]
		public bool ShowVCAs { get; set; }

		public SoundInfoCommand(FMOD.Studio.System system)
		{
			this.system	=	system;
		}

		public object Execute()
		{
			FmodExt.ERRCHECK( system.getBankList(out var bankList) );

			Log.Message("");
			Log.Message("-------- Sound System Info --------");

			system.getParameterDescriptionList( out var paramDescs );

			foreach ( var param in paramDescs )
			{
				Log.Debug($"{(string)param.name} [{param.minimum}..{param.maximum}] {param.flags}");
			}



			for (int i=0; i<bankList.Length; i++)
			{
				var bank = bankList[i];

				bank.getPath(out var bankPath);
				Log.Message($"Bank #{i}: {bankPath}");

				bank.getBusList( out var busList );

				for (int j=0; j<busList.Length; j++)
				{
					var bus = busList[j];
					bus.getPath(out var busPath);
					Log.Debug($"Bus #{j}: {busPath}");

					bus.getChannelGroup(out var cg);

					cg.getNumDSPs(out int numDSPs);

					for (int k=0; k<numDSPs; k++)
					{
						cg.getDSP(k, out DSP dsp);
						dsp.getInfo(out string name, out var version, out _, out _, out _);
						Log.Debug($" - DSP {k,2} : {name}, v{version}");
					}
				}

				bank.getEventList( out var eventList );

				for (int j=0; j<eventList.Length; j++)
				{
					var _event		=	eventList[j];
					_event.is3D			( out var is3D		 );
					_event.isOneshot	( out var isOneshot	 );
					_event.isStream		( out var isStream	 );
					_event.isSnapshot	( out var isSnapshot );
					_event.getPath		( out var eventPath	 );
					var _3d			=	is3D		? "3D"		: "";
					var oneshot		=	isOneshot	? "oneshot"	: "";
					var stream		=	isStream	? "stream"	: "";
					var snapshot	=	isSnapshot	? "snapshot": "";

					Log.Debug($"Event {j,3}: {eventPath,-50} {_3d,3} {oneshot,7} {stream,6} {snapshot,8}");
				}
			}

			Log.Message("----------------");
			

			return null;
		}
	}
}
