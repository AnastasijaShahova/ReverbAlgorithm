using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using ##########Mathematics;
using ##########.Engine.Graphics.Scenes;
using ##########.Engine.Materials;

namespace ##########.Engine.Audio
{
	public class AcousticMaterialSettings
	{
		public const int OctaveBandsCount = 9;

		readonly string name;
		// nine bands from 31.25 Hz to 8000 Hz
		readonly float[] absorptionCoefficients;

		public const int	TLBandsCount	=	3;
		public const float	TL_250Hz		=	20f;
		public const float	TL_1000Hz		=	20f;
		public const float	TL_4000Hz		=	20f;

		//	three bands, similar to air absorbtion
		//	250 Hz, 1000 Hz, 4000 Hz
		readonly float[] transmitionLoss;


		public AcousticMaterialSettings( string name, params float[] absorptionCoefficients )
		{
			if (absorptionCoefficients.Length!=OctaveBandsCount)
			{
				throw new ArgumentOutOfRangeException($"size of {nameof(absorptionCoefficients)} must be {OctaveBandsCount}");
			}

			this.name						=	name;
			this.absorptionCoefficients		=	absorptionCoefficients.ToArray();

			//	something average for now
			//	TODO -- provide more accurate transmission values
			//	http://engineeronadisk.com/notes_mechanic/sounda9.html
			this.transmitionLoss			=	new[] { 20f, 30f, 40f };
		}


		public string Name => name;


		public float GetAbsorption(int band)
		{
			if (band < 0 || band >= OctaveBandsCount) throw new ArgumentOutOfRangeException(nameof(band));

			return absorptionCoefficients[band];
		}


		public float GetTransmissionLoss(int band)
		{
			throw new NotImplementedException();
			//if (band < 0 || band >= TLBandsCount) throw new ArgumentOutOfRangeException(nameof(band));
			//return transmitionLoss[band];
		}


		public Color3 GetAcousticColor()
		{
			float r = ( absorptionCoefficients[0] + absorptionCoefficients[1] + absorptionCoefficients[2] ) / 3;
			float g = ( absorptionCoefficients[3] + absorptionCoefficients[4] + absorptionCoefficients[5] ) / 3;
			float b = ( absorptionCoefficients[6] + absorptionCoefficients[7] + absorptionCoefficients[8] ) / 3;

			return new Color3( 1-r, 1-g, 1-b );
		}


		public static float GetFrequency(int band)
		{
			switch (band)
			{
				case 0: return    31.25f;
				case 1: return    62.50f;
				case 2: return   125;
				case 3: return   250;
				case 4: return   500;
				case 5: return  1000;
				case 6: return  2000;
				case 7: return  4000;
				case 8: return  8000;
				default: throw new ArgumentOutOfRangeException(nameof(band));
			}
		}


		public static AcousticMaterialSettings GetMaterial(AcousticMaterial id )
		{
			switch (id)
			{
				case AcousticMaterial.Default:		return Metal;
				case AcousticMaterial.Metal:		return Metal;
				case AcousticMaterial.Concrete:		return ConcreteBlockRough;
				case AcousticMaterial.Sand:			return ConcreteBlockCoarse;
				case AcousticMaterial.Rock:			return ConcreteBlockCoarse;
				case AcousticMaterial.Flesh:		return People;
				case AcousticMaterial.Wood:			return ParquetOnConcrete;
				case AcousticMaterial.Tile:			return Marble;
				case AcousticMaterial.Plaster:		return PlasterSmooth;
				case AcousticMaterial.Vinil:		return LinoleumOnConcrete;
				case AcousticMaterial.Grass:		return Soil;
				case AcousticMaterial.Dirt:			return Soil;
				case AcousticMaterial.Water:		return WaterOrIceSurface;
				case AcousticMaterial.Ice:			return WaterOrIceSurface;
				case AcousticMaterial.Brick:		return BrickBare;
				case AcousticMaterial.Marble:		return Marble;
				case AcousticMaterial.Fabric:		return Drapery;
				case AcousticMaterial.Glass:		return GlassThin;
				default: throw new ArgumentException(nameof(id));
			}
		}

		//																																	31		62		125		250		500		1000	2000	4000	8000
		public static readonly AcousticMaterialSettings People					= new AcousticMaterialSettings("People"					, 0.250f, 0.250f, 0.350f, 0.350f, 0.420f, 0.460f, 0.500f, 0.500f, 1.000f );
		public static readonly AcousticMaterialSettings Soil					= new AcousticMaterialSettings("Soil"					, 0.070f, 0.070f, 0.100f, 0.400f, 0.600f, 0.500f, 0.500f, 0.400f, 0.400f );
		public static readonly AcousticMaterialSettings Drapery 				= new AcousticMaterialSettings("Drapery"				, 0.070f, 0.070f, 0.070f, 0.310f, 0.490f, 0.750f, 0.700f, 0.600f, 1.000f );
		public static readonly AcousticMaterialSettings Transparent				= new AcousticMaterialSettings("Transparent"			, 1.000f, 1.000f, 1.000f, 1.000f, 1.000f, 1.000f, 1.000f, 1.000f, 1.000f );
		public static readonly AcousticMaterialSettings AcousticCeilingTiles	= new AcousticMaterialSettings("AcousticCeilingTiles"	, 0.672f, 0.675f, 0.700f, 0.660f, 0.720f, 0.920f, 0.880f, 0.750f, 1.000f );
		public static readonly AcousticMaterialSettings BrickBare				= new AcousticMaterialSettings("BrickBare"				, 0.030f, 0.030f, 0.030f, 0.030f, 0.030f, 0.040f, 0.050f, 0.070f, 0.140f );
		public static readonly AcousticMaterialSettings BrickPainted			= new AcousticMaterialSettings("BrickPainted"			, 0.006f, 0.007f, 0.010f, 0.010f, 0.020f, 0.020f, 0.020f, 0.030f, 0.060f );
		public static readonly AcousticMaterialSettings Rock					= new AcousticMaterialSettings("Rock"					, 0.360f, 0.360f, 0.360f, 0.440f, 0.310f, 0.290f, 0.390f, 0.250f, 0.500f );
		public static readonly AcousticMaterialSettings ConcreteBlockCoarse		= new AcousticMaterialSettings("ConcreteBlockCoarse"	, 0.360f, 0.360f, 0.360f, 0.440f, 0.310f, 0.290f, 0.390f, 0.250f, 0.500f );
		public static readonly AcousticMaterialSettings ConcreteBlockRough		= new AcousticMaterialSettings("ConcreteBlockRough"		, 0.010f, 0.010f, 0.010f, 0.020f, 0.040f, 0.060f, 0.080f, 0.100f, 0.200f );
		public static readonly AcousticMaterialSettings ConcreteBlockPainted	= new AcousticMaterialSettings("ConcreteBlockPainted"	, 0.092f, 0.090f, 0.100f, 0.050f, 0.060f, 0.070f, 0.090f, 0.080f, 0.160f );
		public static readonly AcousticMaterialSettings CurtainHeavy			= new AcousticMaterialSettings("CurtainHeavy"			, 0.073f, 0.106f, 0.140f, 0.350f, 0.550f, 0.720f, 0.700f, 0.650f, 1.000f );
		public static readonly AcousticMaterialSettings FiberGlassInsulation	= new AcousticMaterialSettings("FiberGlassInsulation"	, 0.193f, 0.220f, 0.220f, 0.820f, 0.990f, 0.990f, 0.990f, 0.990f, 1.000f );
		public static readonly AcousticMaterialSettings GlassThin				= new AcousticMaterialSettings("GlassThin"				, 0.180f, 0.169f, 0.180f, 0.060f, 0.040f, 0.030f, 0.020f, 0.020f, 0.040f );
		public static readonly AcousticMaterialSettings GlassThick				= new AcousticMaterialSettings("GlassThick"				, 0.350f, 0.350f, 0.350f, 0.250f, 0.180f, 0.120f, 0.070f, 0.040f, 0.080f );
		public static readonly AcousticMaterialSettings Grass					= new AcousticMaterialSettings("Grass"					, 0.050f, 0.050f, 0.150f, 0.250f, 0.400f, 0.550f, 0.600f, 0.600f, 0.600f );
		public static readonly AcousticMaterialSettings LinoleumOnConcrete		= new AcousticMaterialSettings("LinoleumOnConcrete"		, 0.020f, 0.020f, 0.020f, 0.030f, 0.030f, 0.030f, 0.030f, 0.020f, 0.040f );
		public static readonly AcousticMaterialSettings Marble					= new AcousticMaterialSettings("Marble"					, 0.010f, 0.010f, 0.010f, 0.010f, 0.010f, 0.010f, 0.020f, 0.020f, 0.040f );
		public static readonly AcousticMaterialSettings Metal					= new AcousticMaterialSettings("Metal"					, 0.030f, 0.035f, 0.040f, 0.040f, 0.050f, 0.050f, 0.050f, 0.070f, 0.090f );
		public static readonly AcousticMaterialSettings ParquetOnConcrete		= new AcousticMaterialSettings("ParquetOnConcrete"		, 0.028f, 0.030f, 0.040f, 0.040f, 0.070f, 0.060f, 0.060f, 0.070f, 0.140f );
		public static readonly AcousticMaterialSettings ParquetOnJoists			= new AcousticMaterialSettings("ParquetOnJoists"		, 0.100f, 0.120f, 0.150f, 0.110f, 0.100f, 0.070f, 0.060f, 0.060f, 0.120f );
		public static readonly AcousticMaterialSettings PlasterRough			= new AcousticMaterialSettings("PlasterRough"			, 0.017f, 0.018f, 0.020f, 0.030f, 0.040f, 0.050f, 0.040f, 0.030f, 0.060f );
		public static readonly AcousticMaterialSettings PlasterSmooth			= new AcousticMaterialSettings("PlasterSmooth"			, 0.011f, 0.012f, 0.013f, 0.015f, 0.020f, 0.030f, 0.040f, 0.050f, 0.100f );
		public static readonly AcousticMaterialSettings PlywoodPanel			= new AcousticMaterialSettings("PlywoodPanel"			, 0.400f, 0.340f, 0.280f, 0.220f, 0.170f, 0.090f, 0.100f, 0.110f, 0.220f );
		public static readonly AcousticMaterialSettings PolishedConcreteOrTile	= new AcousticMaterialSettings("PolishedConcreteOrTile"	, 0.008f, 0.008f, 0.010f, 0.010f, 0.015f, 0.020f, 0.020f, 0.020f, 0.040f );
		public static readonly AcousticMaterialSettings Sheetrock				= new AcousticMaterialSettings("Sheetrock"				, 0.290f, 0.279f, 0.290f, 0.100f, 0.050f, 0.040f, 0.070f, 0.090f, 0.180f );
		public static readonly AcousticMaterialSettings WaterOrIceSurface		= new AcousticMaterialSettings("WaterOrIceSurface"		, 0.006f, 0.006f, 0.008f, 0.008f, 0.013f, 0.015f, 0.020f, 0.025f, 0.050f );
		public static readonly AcousticMaterialSettings WoodCeiling				= new AcousticMaterialSettings("WoodCeiling"			, 0.150f, 0.147f, 0.150f, 0.110f, 0.100f, 0.070f, 0.060f, 0.070f, 0.140f );
		public static readonly AcousticMaterialSettings WoodPanel				= new AcousticMaterialSettings("WoodPanel"				, 0.280f, 0.280f, 0.280f, 0.220f, 0.170f, 0.090f, 0.100f, 0.110f, 0.220f );
		public static readonly AcousticMaterialSettings Uniform					= new AcousticMaterialSettings("Uniform"				, 0.500f, 0.500f, 0.500f, 0.500f, 0.500f, 0.500f, 0.500f, 0.500f, 0.500f );
	}
}
