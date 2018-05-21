namespace J
{
	using System;
	using System.Linq;
	using UniRx;
	using Object = UnityEngine.Object;

	public partial class AssetLoaderInstance
	{
		IObservable<Object> LoadCore(AssetEntry entry)
		{
			return GetAssetBundleWithDependencies(entry.BundleEntry).ContinueWith(bundle =>
			{
				if (entry.LoadMethod == LoadMethod.Single)
					return bundle.LoadAssetAsync(entry.AssetName, entry.AssetType)
						.AsAsyncOperationObservable().Select(req => req.asset);
				if (entry.LoadMethod == LoadMethod.Multi)
					return bundle.LoadAssetWithSubAssetsAsync(entry.AssetName, entry.AssetType)
						.AsAsyncOperationObservable().SelectMany(req => req.allAssets);
				throw new Exception("Unknown LoadMethod. " + entry.LoadMethod);
			});
		}

		public IObservable<Object> Load(AssetEntry entry) => (SimulationMode ? AssetGraphLoader.Load : LoadCore)(entry);
	}
}
