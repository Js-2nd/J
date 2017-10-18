#if UNITY_EDITOR
namespace J
{
	using System.Linq;
	using UniRx;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.AssetBundles.GraphTool;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		ReplaySubject<Object> LoadFromGraphTool(AssetEntry entry)
		{
			var cache = new ReplaySubject<Object>();
			var paths = AssetBundleBuildMap.GetBuildMap().GetAssetPathsFromAssetBundleAndAssetName(entry.BundleName, entry.AssetName);
			if (paths == null || paths.Length == 0)
			{
				throw new System.Exception(string.Format("Asset not found. {0}", entry));
			}
			AssetDatabase.LoadAllAssetsAtPath(paths[0])
				.Where(obj => entry.AssetType.IsAssignableFrom(obj.GetType()))
				.ToObservable()
				.DelayFrame(1)
				.Subscribe(cache);
			return cache;
		}
	}
}
#endif
