using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ##########;
using ##########Mathematics;
using ##########Configuration;
using ##########.Engine.Audio;
using ##########.Engine.Common;
using ##########.Widgets.Advanced;
using ##########.Engine.Graphics.Scenes;

namespace ##########.Engine.Audio 
{
	public enum Falloff
	{
		Constant,
		InverseQuadraticSmooth,
	}

	[ConfigClass]
	public sealed partial class SoundSystem : GameComponent 
	{
		public const int T60ApproxThresholdDB		= -60;
		public const int T60ApproxMinSampleCount	= 10;

		/*---------------------------------------------------------------------
		 *	Mixer :
		---------------------------------------------------------------------*/

		[Config]
		[AECategory("Mixer")]
		[AESlider(0,1, 0.1f, 0.01f)]
		public static float MasterVolume { get; set; } = 1.0f;

		[Config]
		[AECategory("Mixer")]
		[AESlider(0,1, 0.1f, 0.01f)]
		public static float SFXVolume { get; set; } = 1.0f;

		[Config]
		[AECategory("Mixer")]
		[AESlider(0,1, 0.1f, 0.01f)]
		public static float MusicVolume { get; set; } = 1.0f;

		[Config]
		[AECategory("Mixer")]
		[AESlider(0,1, 0.1f, 0.01f)]
		public static float VoiceVolume { get; set; } = 1.0f;

		[Config]
		[AECategory("Mixer")]
		[AESlider(0,1, 0.1f, 0.01f)]
		public static float UIVolume { get; set; } = 1.0f;

		/*---------------------------------------------------------------------
		 *	Absorption and transmission loss :
		---------------------------------------------------------------------*/

		[Config]
		[AECategory("Air Absorption")]
		[AESlider(1, 100, 1f, 0.125f)]
		public static float AirAbsorptionScale { get; set; } = 3.125f;

		[Config]
		[AESlider(0, 10, 1f, 0.01f)]
		public static float TransmissionLossScale { get; set; } = 1;

		/*---------------------------------------------------------------------
		 *	Obsolete
		---------------------------------------------------------------------*/

		/// <summary>
		/// Overall distance scale. Default = 1.
		/// </summary>
		[Config]
		[Obsolete("Check current distance scale in game and FMOD")]
		public static float DistanceScale { get; set; } = 1.0f;

		/// <summary>
		/// Overall doppler scale. Default = 1;
		/// </summary>
		[Config]
		[Obsolete("Check current doppler scale in game and FMOD")]
		public static float DopplerScale { get; set; } = 1;

		/// <summary>
		/// Global speed of sound. Default = 343.5f;
		/// </summary>
		[Config]
		[Obsolete("Check current velocity scale in game and FMOD")]
		public static float SpeedOfSound { get; set; } = 343.5f;
	}
}
