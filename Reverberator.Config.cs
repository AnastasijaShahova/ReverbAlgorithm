using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ##########;
using ##########Configuration;
using ##########Mathematics;
using ##########.Engine.Graphics.Scenes;
using ##########.Engine.Materials;
using ##########.Widgets.Advanced;

namespace ##########.Engine.Audio
{
	public enum EarlyTimeEstimationMethod
	{
		WeightedAverage,
		WeightedAverageDB,
		NearFieldDecay,
	}

	[ConfigClass]
	public partial class Reverberator
	{
		[Config]
		[AECategory("Reverberation")]
		[AESlider(1, 100, 1f, 0.125f)]
		public static float AirAbsorptionScale { get; set; } = 3;

		[Config]
		[AECategory("Reverberation")]
		public static AcousticMaterial AcousticMaterialOverride { get; set; } = AcousticMaterial.Default;

		[Config]
		[AECategory("Reverberation")]
		[AESlider(8,256, 16, 1)]
		public static int ReverbMaxBounces { get; set; } = 64;

		[Config]
		[AECategory("Reverberation")]
		[AESlider(8,256, 16, 1)]
		public static int ReverbRaysCount { get; set; } = 2048;

		[Config]
		[AECategory("Reverberation")]
		[AESlider(0, 1, 0.1f, 0.01f)]
		public static float DiffuseFactorTracing { get; set; } = 0.3f;

		[Config]
		[AECategory("Reverberation")]
		[AESlider(0, 1, 0.1f, 0.01f)]
		public static float DiffuseFactorHearing { get; set; } = 0.3f;

		[Config]
		[AECategory("Reverberation")]
		[AESlider(-1, 1, 0.1f, 0.01f)]
		public static float FloorCeilingBalance { get; set; } = 0.8f;

		[Config]
		[AECategory("Reverberation")]
		public static bool OccludeResponse { get; set; } = true;

		[Config]
		[AECategory("Reverberation")]
		public static Falloff ResponseFalloff { get; set; } = Falloff.InverseQuadraticSmooth;

		[Config]
		[AECategory("Reverberation")]
		public static float ResponseFalloffScale { get; set; } = 1;

		[Config]
		[AECategory("Reverberation")]
		[AESlider(8,256,8, 1)]
		public static int HistogramBinCount { get; set; } = 256;

		[Config]
		[AECategory("Reverberation")]
		[AESlider(0,10,1, 0.1f)]
		public static float T60MaxTime { get; set; } = 10;

		[Config]
		[AECategory("Reverberation")]
		public static bool ShowReverbParams { get; set; } = false;

		[Config]
		[AECategory("Tuning")]
		[AESlider(20, 20000, 100, 1f)]
		public static float ReverbHighCut { get; set; } = 5000;

		[Config]
		[AECategory("Tuning")]
		public static float NearTimeThreshold { get; set; } = 0.1f;

		[Config]
		[AECategory("Tuning")]
		public static EarlyTimeEstimationMethod EarlyTimeMethod { get; set; } = EarlyTimeEstimationMethod.NearFieldDecay;
	}
}
