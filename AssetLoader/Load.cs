using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace J
{
	using UnityEngine;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		Dictionary<AssetEntry, ReplaySubject<Object>> m_AssetCache = new Dictionary<AssetEntry, ReplaySubject<Object>>();

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
#if UNITY_EDITOR
			if (m_SimulationMode)
			{
				method = LoadFromGraphTool;
			}
#endif
			return m_AssetCache.GetOrAdd(entry, method);
		}
	}
}
