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
		public string ETag { get; set; }
		public string LastModified { get; set; }

		public override IObservable<UnityWebRequest> FetchHead(IProgress<float> progress = null)
		{
			return Observable.Defer(() =>
			{
				if (IsHeadFetched) return ReturnNull.ReportOnCompleted(progress);
				return (Hash.isValid ? AssetLoader.WhenCacheReady() : Observable.ReturnUnit()).ContinueWith(_ =>
				{
					if (Hash.isValid && Caching.IsVersionCached(Url, Hash))
					{
						IsHeadFetched = true;
						IsDownloaded = true;
						return ReturnNull.ReportOnCompleted(progress);
					}
					return UnityWebRequest.Head(Url).SendAsObservable(progress, ETag, LastModified).Do(req =>
					{
						IsHeadFetched = true;
						if (req.responseCode == 304) IsDownloaded = true;
						Size = req.GetContentLengthNum();
					});
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
				return request.SendAsObservable(progress).Do(_ => IsDownloaded = true);
			});
		}
	}
}
