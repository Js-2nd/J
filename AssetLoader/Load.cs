namespace J
{
	using System;
	using UniRx;

	partial class AssetLoaderInstance
	{
		IObservable<UnityEngine.Object> LoadCore(AssetEntry entry)
		{
			return GetAssetBundleWithDependencies(entry.BundleEntry).ContinueWith(bundle =>
			{
				switch (entry.LoadMethod)
				{
					case LoadMethod.Single:
						return bundle.LoadAssetAsync(entry.AssetName, entry.AssetType)
							.AsAsyncOperationObservable().Select(req => req.asset);
					case LoadMethod.Multi:
						return bundle.LoadAssetWithSubAssetsAsync(entry.AssetName, entry.AssetType)
							.AsAsyncOperationObservable().SelectMany(req => req.allAssets);
					default:
						throw new Exception("Unknown LoadMethod. " + entry.LoadMethod);
				}
			});
		}

		public LoadDelegate Load { get; private set; }

		void UpdateLoadMethod()
		{
			if (IsSimulationEnabled)
			{
				switch (SimulationMode)
				{
					case Simulation.AssetDatabase:
						Load = AssetDatabaseLoader.Load; return;
					case Simulation.AssetGraph:
						Load = AssetGraphLoader.Load; return;
				}
			}
			Load = LoadCore;
		}

		public bool IsSimulationEnabled
		{
			get
			{
				switch (SimulationMode)
				{
					case Simulation.AssetDatabase: return AssetDatabaseLoader.IsAvailable;
					case Simulation.AssetGraph: return AssetGraphLoader.IsAvailable;
					default: return false;
				}
			}
		}

		public enum Simulation
		{
			Disable = 0,
			AssetDatabase = 1,
			AssetGraph = 2,
		}
	}
}
