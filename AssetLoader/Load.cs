namespace J
{
	using System;
	using UniRx;
	using Object = UnityEngine.Object;

	partial class AssetLoaderInstance
	{
		IObservable<Object> LoadCore(AssetEntry entry)
		{
			return WhenManifestLoaded().ContinueWith(_ =>
				GetAssetBundleWithDependencies(entry.BundleEntry).ContinueWith(reference =>
					entry.LoadFrom(reference.Bundle).Finally(reference.Dispose)));
		}

		public LoadDelegate Load { get; private set; }

		void UpdateLoadMethod()
		{
			if (IsSimulationEnabled)
			{
				switch (m_Simulation)
				{
					case AssetSimulation.AssetDatabase:
						Load = AssetDatabaseLoader.Load; return;
					case AssetSimulation.AssetGraph:
						Load = AssetGraphLoader.Load; return;
				}
			}
			Load = LoadCore;
		}

		public bool IsSimulationEnabled
		{
			get
			{
				switch (m_Simulation)
				{
					case AssetSimulation.AssetDatabase: return AssetDatabaseLoader.IsAvailable;
					case AssetSimulation.AssetGraph: return AssetGraphLoader.IsAvailable;
					default: return false;
				}
			}
		}
	}

	public enum AssetSimulation
	{
		Disable = 0,
		AssetDatabase = 1,
		AssetGraph = 2,
	}

	partial class AssetLoader
	{
		public static bool IsSimulationEnabled => Instance && Instance.IsSimulationEnabled;

		public static IObservable<Object> Load(string bundleName, string assetName = null, Type assetType = null) =>
			Instance.Load(new AssetEntry(bundleName, assetName, assetType, LoadMethod.Single));
		public static IObservable<Object> Load(string bundleName, Type assetType) =>
			Instance.Load(new AssetEntry(bundleName, null, assetType, LoadMethod.Single));
		public static IObservable<T> Load<T>(string bundleName, string assetName = null) where T : Object =>
			Instance.Load(new AssetEntry(bundleName, assetName, typeof(T), LoadMethod.Single)).Select(obj => (T)obj);

		public static IObservable<Object> LoadMulti(string bundleName, string assetName = null, Type assetType = null) =>
			Instance.Load(new AssetEntry(bundleName, assetName, assetType, LoadMethod.Multi));
		public static IObservable<Object> LoadMulti(string bundleName, Type assetType) =>
			Instance.Load(new AssetEntry(bundleName, null, assetType, LoadMethod.Multi));
		public static IObservable<T> LoadMulti<T>(string bundleName, string assetName = null) where T : Object =>
			Instance.Load(new AssetEntry(bundleName, assetName, typeof(T), LoadMethod.Multi)).Select(obj => (T)obj);
	}
}
