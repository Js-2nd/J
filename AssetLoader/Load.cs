namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using Object = UnityEngine.Object;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		readonly Dictionary<AssetEntry, ReplaySubject<Object>> m_AssetCache = new Dictionary<AssetEntry, ReplaySubject<Object>>();

		ReplaySubject<Object> LoadCore(AssetEntry entry)
		{
			var cache = new ReplaySubject<Object>();
			GetAssetBundleWithDependencies(entry.ToBundleEntry())
				.ContinueWith(bundle => bundle.LoadAssetWithSubAssetsAsync(entry.AssetName, entry.AssetType).AsAsyncOperationObservable())
				.SelectMany(req => req.allAssets)
				.Subscribe(cache);
			return cache;
		}

		public IObservable<Object> Load(AssetEntry entry)
		{
			Func<AssetEntry, ReplaySubject<Object>> method = LoadCore;
			if (SimulationMode)
			{
				method = LoadFromGraphTool;
			}
			return m_AssetCache.GetOrAdd(entry, method);
		}
	}
}
