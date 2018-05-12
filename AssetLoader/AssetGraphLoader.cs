namespace J
{
#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.Compilation;
#endif
	using System;
	using System.Linq;
	using UniRx;
	using Object = UnityEngine.Object;
	using static AssetLoaderInstance;

	public class AssetGraphLoader
	{
		delegate string[] GetAssetPathsDelegate(string bundleName, string assetName);

		public static readonly bool IsValid;
		public static readonly Func<AssetEntry, IObservable<Object>> Load;

		static AssetGraphLoader()
		{
			IsValid = false;
			Load = _ => Observable.Throw<Object>(new NotSupportedException());
#if UNITY_EDITOR
			var type = CompilationPipeline.GetAssemblies()
				.Select(asm => Type.GetType("UnityEngine.AssetGraph.AssetBundleBuildMap, " + asm.name))
				.FirstOrDefault(t => t != null);
			if (type != null)
			{
				var map = type.GetMethod("GetBuildMap").Invoke(null, null);
				var getAssetPaths = (GetAssetPathsDelegate)Delegate.CreateDelegate(typeof(GetAssetPathsDelegate), map, type.GetMethod("GetAssetPathsFromAssetBundleAndAssetName"));
				IsValid = true;
				Load = entry =>
				{
					var path = getAssetPaths(entry.BundleName, entry.AssetName)?.FirstOrDefault();
					if (string.IsNullOrEmpty(path))
						return Observable.Throw<Object>(new Exception("Asset not found. " + entry));
					if (entry.LoadMethod == LoadMethod.Single)
						return Observable.Return(AssetDatabase.LoadAssetAtPath(path, entry.AssetType));
					if (entry.LoadMethod == LoadMethod.Multi)
						return AssetDatabase.LoadAllAssetsAtPath(path)
							.Where(obj => entry.AssetType.IsAssignableFrom(obj.GetType()))
							.ToObservable();
					throw new Exception("Unknown LoadMethod. " + entry.LoadMethod);
				};
			}
#endif
		}
	}
}
