namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine;
	using Object = UnityEngine.Object;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		readonly Dictionary<AssetEntry, ReplaySubject<Object>> m_AssetCache = new Dictionary<AssetEntry, ReplaySubject<Object>>();

		ReplaySubject<Object> LoadCore(AssetEntry entry)
		{
			var cache = new ReplaySubject<Object>();
			Func<AssetBundle, IObservable<Object>> loadFunc;
			if (entry.LoadMethod == LoadMethod.Single)
			{
				loadFunc = bundle => bundle.LoadAssetAsync(entry.AssetName, entry.AssetType).AsAsyncOperationObservable()
					.Select(req => req.asset);
			}
			else if (entry.LoadMethod == LoadMethod.Multi)
			{
				loadFunc = bundle => bundle.LoadAssetWithSubAssetsAsync(entry.AssetName, entry.AssetType).AsAsyncOperationObservable()
					.SelectMany(req => req.allAssets);
			}
			else
			{
				throw new Exception(string.Format("Unknown LoadMethod. {0}", entry.LoadMethod));
			}

			GetAssetBundleWithDependencies(entry.ToBundleEntry())
				.ContinueWith(loadFunc)
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
