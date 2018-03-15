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

	class AssetGraphLoader
	{
		delegate string[] GetAssetPathsDelegate(string assetbundleName, string assetName);

		public static readonly bool IsValid;
		public static readonly LoadDelegate Load;

		static AssetGraphLoader()
		{
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
					var paths = getAssetPaths(entry.BundleName, entry.AssetName);
					if (paths == null || paths.Length == 0)
						return Observable.Throw<Object>(new Exception("Asset not found. " + entry));
					if (entry.LoadMethod == LoadMethod.Single)
						return Observable.Return(AssetDatabase.LoadAssetAtPath(paths[0], entry.AssetType));
					else if (entry.LoadMethod == LoadMethod.Multi)
						return AssetDatabase.LoadAllAssetsAtPath(paths[0])
							.Where(obj => entry.AssetType.IsAssignableFrom(obj.GetType()))
							.ToObservable();
					else throw new Exception("Unknown LoadMethod. " + entry.LoadMethod);
				};
			}
#endif
			Load = _ => Observable.Throw<Object>(new NotSupportedException());
		}
	}
}
