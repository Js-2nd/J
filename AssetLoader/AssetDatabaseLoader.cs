#if UNITY_EDITOR
using UnityEditor;
#endif

namespace J
{
	using J.Internal;
	using System;
	using System.IO;
	using System.Linq;
	using UniRx;

	public static class AssetDatabaseLoader
	{
		public static readonly bool IsAvailable;
		public static readonly GetAssetPathsDelegate GetAssetPaths;
		public static readonly LoadAssetDelegate Load;

		static AssetDatabaseLoader()
		{
#if UNITY_EDITOR
			IsAvailable = true;
			GetAssetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName;
			Load = ToLoadMethod(GetAssetPaths);
#endif
		}

		public static LoadAssetDelegate ToLoadMethod(GetAssetPathsDelegate getAssetPaths)
		{
#if UNITY_EDITOR
			if (getAssetPaths == null) throw new ArgumentNullException(nameof(getAssetPaths));
			return entry =>
			{
				string path = getAssetPaths(entry.NormBundleName, entry.AssetName)?.FirstOrDefault();
				if (string.IsNullOrEmpty(path))
					return Observable.Throw<UnityEngine.Object>(
						new FileNotFoundException($"Asset not found. {entry}"),
						Scheduler.MainThreadIgnoreTimeScale);
				switch (entry.LoadMethod)
				{
					case LoadMethod.Single:
						return Observable.Return(AssetDatabase.LoadAssetAtPath(path, entry.AssetType),
							Scheduler.MainThreadIgnoreTimeScale);
					case LoadMethod.Multi:
						return AssetDatabase.LoadMainAssetAtPath(path).ToSingleEnumerable()
							.Concat(AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
								.Where(AssetDatabase.IsForeignAsset))
							.Where(obj => entry.AssetType.IsInstanceOfType(obj))
							.ToObservable(Scheduler.MainThreadIgnoreTimeScale);
					default: throw new ArgumentException("Unknown LoadMethod. " + entry.LoadMethod);
				}
			};
#else
			throw new NotSupportedException();
#endif
		}
	}
}
