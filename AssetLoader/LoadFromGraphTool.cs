namespace J
{
#if UNITY_EDITOR
	using UnityEditor;
#endif

	using System;
	using System.Linq;
	using UniRx;
	using UnityEngine.AssetBundles.GraphTool;
	using Object = UnityEngine.Object;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		ReplaySubject<Object> LoadFromGraphTool(AssetEntry entry)
		{
#if UNITY_EDITOR
			var cache = new ReplaySubject<Object>();
			var paths = AssetBundleBuildMap.GetBuildMap().GetAssetPathsFromAssetBundleAndAssetName(entry.BundleName, entry.AssetName);
			if (paths == null || paths.Length == 0)
			{
				throw new Exception(string.Format("Asset not found. {0}", entry));
			}
			AssetDatabase.LoadAllAssetsAtPath(paths[0])
				.Where(obj => entry.AssetType.IsAssignableFrom(obj.GetType()))
				.ToObservable()
				.DelayFrame(1)
				.Subscribe(cache);
			return cache;
#else
			throw new NotImplementedException();
#endif
		}
	}
}
