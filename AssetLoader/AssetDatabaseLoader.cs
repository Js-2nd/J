#if UNITY_EDITOR
using UnityEditor;
#endif

namespace J
{
	using System;
	using System.Linq;
	using UniRx;
	using static AssetLoaderInstance;

	public static class AssetDatabaseLoader
	{
		public static readonly bool IsAvailable;
		public static readonly GetAssetPathsDelegate GetAssetPaths;
		public static readonly LoadDelegate Load;

		static AssetDatabaseLoader()
		{
#if UNITY_EDITOR
			IsAvailable = true;
			GetAssetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName;
			Load = ToLoadMethod(GetAssetPaths);
#endif
		}

		public static LoadDelegate ToLoadMethod(GetAssetPathsDelegate getAssetPaths)
		{
#if UNITY_EDITOR
			if (getAssetPaths == null) throw new ArgumentNullException(nameof(getAssetPaths));
			return entry =>
			{
				string path = getAssetPaths(entry.NormedBundleName, entry.AssetName)?.FirstOrDefault();
				if (string.IsNullOrEmpty(path))
					return Observable.Throw<UnityEngine.Object>(new Exception("Asset not found. " + entry), Scheduler.MainThreadIgnoreTimeScale);
				switch (entry.LoadMethod)
				{
					case LoadMethod.Single:
						return Observable.Return(AssetDatabase.LoadAssetAtPath(path, entry.AssetType), Scheduler.MainThreadIgnoreTimeScale);
					case LoadMethod.Multi:
						return AssetDatabase.LoadAllAssetsAtPath(path)
							.Where(obj => entry.AssetType.IsInstanceOfType(obj))
							.ToObservable(Scheduler.MainThreadIgnoreTimeScale);
					default:
						throw new Exception("Unknown LoadMethod. " + entry.LoadMethod);
				}
			};
#else
			throw new NotSupportedException();
#endif
		}
	}
}
