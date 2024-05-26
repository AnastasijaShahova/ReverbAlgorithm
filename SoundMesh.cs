using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ##########;
using ##########Mathematics;
using ##########.Engine.Graphics.Scenes;
using Native.Embree;

namespace ##########.Engine.Audio
{
	public class SoundMesh
	{
		internal readonly uint ID;

		internal SoundMesh( uint id )
		{
			ID = id;
		}
	}
}
