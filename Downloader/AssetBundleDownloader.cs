#if !UNITY_2018_1_OR_NEWER
using UnityWebRequestAssetBundle = UnityEngine.Networking.UnityWebRequest;
#endif

namespace J
{
	using J.Internal;
	using System;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public sealed class AssetBundleDownloader : Downloader
	{
		public string Url { get; set; }
		public Hash128 Hash { get; set; }
		//public string ETag { get; set; }
		//public string LastModified { get; set; }

		public override IObservable<UnityWebRequest> FetchHead(IProgress<float> progress = null)
		{
			return Observable.Defer(() =>
			{
				if (IsHeadFetched) return ReturnNull.ReportOnCompleted(progress);
				return (Hash.isValid ? AssetLoader.WhenCacheReady() : Observable.ReturnUnit()).ContinueWith(_ =>
				{
					if (Hash.isValid && Caching.IsVersionCached(Url, Hash))
					{
						try { OnDownloaded(null); } // TODO remove try-catch block https://github.com/neuecc/UniRx/issues/311
						catch (Exception ex) { return Observable.Throw<UnityWebRequest>(ex); }
						return ReturnNull.ReportOnCompleted(progress);
					}
					return UnityWebRequest.Head(Url).SendAsObservable(progress/*, ETag, LastModified*/).Do(OnHeadFetched);
				});
			});
		}

		public override IObservable<UnityWebRequest> Download(IProgress<float> progress = null)
		{
			return Observable.Defer(() =>
			{
				var request = Hash.isValid
					? UnityWebRequestAssetBundle.GetAssetBundle(Url, Hash, 0)
					: UnityWebRequestAssetBundle.GetAssetBundle(Url);
				return request.SendAsObservable(progress).Do(OnDownloaded);
			});
		}
	}
}
