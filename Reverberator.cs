using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ##########.Build;
using ##########;
using ##########Configuration;
using ##########Extensions;
using ##########Mathematics;

namespace ##########.Engine.Audio
{
	public partial class Reverberator
	{
		public static bool ExportCSVReverbData = false;

		readonly FMOD.Studio.System	system;
		readonly FMOD.System		core;
		readonly SoundSystem		ss;

		readonly ReverbSimulator	simulator;
		readonly ReverbEstimator	estimator;

		ReverbParams	currentReverbLeft		=	new ReverbParams();
		ReverbParams	currentReverbRight		=	new ReverbParams();
		ReverbParams	targerReverbLeft		=	null;
		ReverbParams	targerReverbRight		=	null;

		readonly ConcurrentQueue<ReverbParams> reverbResultQueueLeft = new ConcurrentQueue<ReverbParams>();
		readonly ConcurrentQueue<ReverbParams> reverbResultQueueRight = new ConcurrentQueue<ReverbParams>();


		public Reverberator( SoundSystem ss )
		{
			this.ss		=	ss;;
			system		=	ss.system;
			system.getCoreSystem( out core );

			simulator	=	new ReverbSimulator();
			estimator	=	new ReverbEstimator(ss);
		}


		bool reverbSimulationDone = true;


		public void Update( GameTime gameTime )
		{
			//	get reverb results :
			while (reverbResultQueueLeft	.TryDequeue(out var result)) 	targerReverbLeft	=	result;
			while (reverbResultQueueRight	.TryDequeue(out var result))	targerReverbRight	=	result;

			//	interpolate reverb parameters :
			currentReverbLeft	 .Seek( gameTime.ElapsedSec, targerReverbLeft		);
			currentReverbRight	 .Seek( gameTime.ElapsedSec, targerReverbRight		);

			//	if scene is set, issue new reverb request :
			if (ss.Scene!=null)
			{
				if (reverbSimulationDone)
				{
					reverbSimulationDone = false;

					ss.Game.GetService<ParallelWorker>().Run( () => ComputeReverb( ss.ListenerPosition, ss.Scene ) );
				}
			}
			else
			{
				//	reset settings otherwice :
				currentReverbLeft.Reset();
				currentReverbRight.Reset();
			}

			//	apply reverb settings :
			currentReverbLeft	 .Apply( ss.system, ReverbChannel.Left,		SoundSystem.BusSFXReverbLeft	);
			currentReverbRight	 .Apply( ss.system, ReverbChannel.Right,	SoundSystem.BusSFXReverbRight	);
		}


		void ComputeReverb( Vector3 listenerPosition, SoundScene scene )
		{
			var plotL		=	ExportCSVReverbData ? "L_" : null;
			var plotR		=	ExportCSVReverbData ? "R_" : null;
			var resetPlot	=	ExportCSVReverbData;

			simulator.SimulateImpulseResponce( scene, listenerPosition );

			var reverbLeft		=	estimator.EstimateReverbSettinngs( simulator, ReverbChannel.Left,		plotL  );
			var reverbRight		=	estimator.EstimateReverbSettinngs( simulator, ReverbChannel.Right,		plotR  );

			reverbResultQueueLeft.Enqueue( reverbLeft );
			reverbResultQueueRight.Enqueue( reverbRight );
				
			if (resetPlot)
			{
				ExportCSVReverbData = false;
			}

			reverbSimulationDone = true;
		}

	}
}
