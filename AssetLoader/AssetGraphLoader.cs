#if UNITY_EDITOR
using UnityEditor.Compilation;
#endif

namespace J
{
	using System;
	using System.Linq;
	using static AssetLoaderInstance;

	public static class AssetGraphLoader
	{
		public static readonly bool IsAvailable;
		public static readonly GetAssetPathsDelegate GetAssetPaths;
		public static readonly LoadDelegate Load;

		static AssetGraphLoader()
		{
#if UNITY_EDITOR
			var type = CompilationPipeline.GetAssemblies()
				.Select(asm => Type.GetType("UnityEngine.AssetGraph.AssetBundleBuildMap, " + asm.name))
				.FirstOrDefault(t => t != null);
			if (type == null) return;
			var map = type.GetMethod("GetBuildMap")?.Invoke(null, null);
			var method = type.GetMethod("GetAssetPathsFromAssetBundleAndAssetName");
			if (map == null || method == null) return;
			IsAvailable = true;
			GetAssetPaths = (GetAssetPathsDelegate)Delegate.CreateDelegate(typeof(GetAssetPathsDelegate), map, method);
			Load = AssetDatabaseLoader.ToLoadDelegate(GetAssetPaths);
#endif
		}
	}
}
