using System;
using UniRx;
using static J.AssetLoaderInstance;

namespace J
{
	using UnityEngine;

	public static class AssetLoader
	{
		public static AssetBundleManifest Manifest
		{
			get { return AssetLoaderInstance.Instance.m_Manifest; }
		}

		public static void LoadManifest(string uri)
		{
			AssetLoaderInstance.Instance.LoadManifest(uri);
		}

		public static IObservable<Unit> WaitForManifestLoaded()
		{
			return AssetLoaderInstance.Instance.WaitForManifestLoaded();
		}

		public static IObservable<AssetBundle> GetAssetBundle(string bundleName)
		{
			return AssetLoaderInstance.Instance.GetAssetBundle(new BundleEntry(bundleName));
		}

		public static IObservable<AssetBundle> GetAssetBundleWithDependencies(string bundleName)
		{
			return AssetLoaderInstance.Instance.GetAssetBundleWithDependencies(new BundleEntry(bundleName));
		}

		public static IObservable<Object> Load(string bundleName)
		{
			return AssetLoaderInstance.Instance.Load(new AssetEntry(bundleName));
		}
		public static IObservable<Object> Load(string bundleName, string assetName)
		{
			return AssetLoaderInstance.Instance.Load(new AssetEntry(bundleName, assetName));
		}
		public static IObservable<Object> Load(string bundleName, Type assetType)
		{
			return AssetLoaderInstance.Instance.Load(new AssetEntry(bundleName, assetType));
		}
		public static IObservable<Object> Load(string bundleName, string assetName, Type assetType)
		{
			return AssetLoaderInstance.Instance.Load(new AssetEntry(bundleName, assetName, assetType));
		}

		public static IObservable<T> Load<T>(string bundleName) where T : Object
		{
			return AssetLoaderInstance.Instance.Load(new AssetEntry(bundleName, typeof(T))).Select(obj => obj as T);
		}
		public static IObservable<T> Load<T>(string bundleName, string assetName) where T : Object
		{
			return AssetLoaderInstance.Instance.Load(new AssetEntry(bundleName, assetName, typeof(T))).Select(obj => obj as T);
		}
	}
}
