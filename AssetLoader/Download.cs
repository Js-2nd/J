namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine;

	partial class AssetLoaderInstance
	{
		public IObservable<BatchDownloader> Download(IEnumerable<string> bundleNames,
			bool includeDependencies = true, BatchDownloader downloader = null)
		{
			return WhenManifestLoaded().Select(_ =>
			{
				var actualNames = bundleNames.Select(NormBundleName).Where(ManifestContains).Select(NormToActualName);
				if (includeDependencies)
					actualNames = actualNames.SelectMany(actualName =>
						actualName.ToSingleEnumerable().Concat(Manifest.GetAllDependencies(actualName)));
				if (downloader == null) downloader = new BatchDownloader();
				foreach (string actualName in actualNames.Distinct())
				{
					var hash = Manifest.GetAssetBundleHash(actualName);
					if (!Caching.IsVersionCached(actualName, hash))
						downloader.Add(new AssetBundleDownloader { Url = RootUrl + actualName, Hash = hash });
				}
				return downloader;
			});
		}
	}

	partial class AssetLoader
	{
		public static IObservable<BatchDownloader> Download(IEnumerable<string> bundleNames,
			bool includeDependencies = true, BatchDownloader downloader = null) =>
			Instance.Download(bundleNames, includeDependencies, downloader);
	}
}
