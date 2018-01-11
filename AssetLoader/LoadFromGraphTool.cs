#if UNITY_EDITOR
using UnityEditor;
#endif

namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine.AssetGraph;
	using Object = UnityEngine.Object;

	public partial class AssetLoaderInstance
	{
		ReplaySubject<Object> LoadFromGraphTool(AssetEntry entry)
		{
#if UNITY_EDITOR
			var cache = new ReplaySubject<Object>();
			var paths = AssetBundleBuildMap.GetBuildMap().GetAssetPathsFromAssetBundleAndAssetName(entry.BundleName, entry.AssetName);
			if (paths == null || paths.Length == 0)
			{
				cache.OnError(new Exception(string.Format("Asset not found. {0}", entry)));
				return cache;
			}

			IEnumerable<Object> source;
			if (entry.LoadMethod == LoadMethod.Single)
			{
				source = AssetDatabase.LoadAssetAtPath(paths[0], entry.AssetType).ToSingleEnumerable();
			}
			else if (entry.LoadMethod == LoadMethod.Multi)
			{
				source = AssetDatabase.LoadAllAssetsAtPath(paths[0])
					.Where(obj => entry.AssetType.IsAssignableFrom(obj.GetType()));
			}
			else
			{
				throw new Exception(string.Format("Unknown LoadMethod. {0}", entry.LoadMethod));
			}
			source.ToObservable().DelayFrame(1).Subscribe(cache);
			return cache;
#else
			throw new NotImplementedException();
#endif
		}
	}
}
