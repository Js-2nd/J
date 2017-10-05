#if UNITY_EDITOR
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.AssetBundles.GraphTool;

namespace J
{
	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		ReplaySubject<Object> LoadFromGraphTool(AssetEntry entry)
		{
			var cache = new ReplaySubject<Object>();
			AssetBundleBuildMap.GetBuildMap()
				.GetAssetPathsFromAssetBundleAndAssetName(entry.BundleName, entry.AssetName)
				.Take(1)
				.SelectMany(path => AssetDatabase.LoadAllAssetsAtPath(path))
				.Where(obj => entry.AssetType.IsAssignableFrom(obj.GetType()))
				.ToObservable()
				.DelayFrame(1)
				.Subscribe(cache);
			return cache;
		}
	}
}
#endif
