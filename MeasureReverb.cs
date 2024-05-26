using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FMOD;
using ##########;
using ##########Shell;
using ##########.Engine.Audio;

namespace ##########.Engine.Audio
{
	public class MeasureReverb : ICommand
	{
		[CommandLineParser.Option]
		[CommandLineParser.Name("export")]
		public bool Export { get; set; }

		public MeasureReverb()
		{
		}

		public object Execute()
		{
			Reverberator.ExportCSVReverbData	=	Export;
			return null;
		}
	}
}
