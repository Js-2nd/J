using System;
using UnityEngine.Networking;

namespace J
{
	using UniRx;
	using UnityEngine;

	public static partial class ExtensionMethods
	{
		public static IObservable<AssetBundle> AsAssetBundleObservable(this UnityWebRequest @this)
		{
			return Observable.Defer(() => @this.Send().AsObservable()
				.Select(_ =>
				{
					try
					{
						AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(@this);
						if (assetBundle == null) throw new Exception();
						return assetBundle;
					}
					catch (Exception)
					{
						throw new Exception(string.Format("AssetBundle not found. {0}", @this.url));
					}
				})
				.Finally(() => @this.Dispose()));
		}
	}
}
