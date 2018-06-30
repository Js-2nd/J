#if !UNITY_2018_1_OR_NEWER
using UnityWebRequestAssetBundle = UnityEngine.Networking.UnityWebRequest;
#endif

namespace J
{
	using System;
	using UnityEngine;
	using UnityEngine.Networking;

	public interface IAssetBundleDownloader : IDownloader
	{
		Hash128 Hash { get; }
	}

	public class AssetBundleDownloader : AssetBundleDownloader<AssetBundleDownloader>
	{
		public static AssetBundleDownloader Create(string url) => new AssetBundleDownloader().SetUrl(url);
		public static AssetBundleDownloader Create(string url, Hash128 hash) => Create(url).SetHash(hash);
	}

	public class AssetBundleDownloader<T> : UnityWebRequestSendOptions<T>,
		IAssetBundleDownloader where T : AssetBundleDownloader<T>
	{
		// TODO need BundleName?
		public string Url { get; set; }
		public Hash128 Hash { get; set; }

		public T SetUrl(string url)
		{
			Url = url;
			return (T)this;
		}
		public T SetHash(Hash128 hash)
		{
			Hash = hash;
			return (T)this;
		}

		public IObservable<UnityWebRequest> Download(IProgress<float> progress = null) =>
			UnityWebRequestAssetBundle.GetAssetBundle(Url, Hash, 0).SendAsObservable(this, progress);
	}
}
