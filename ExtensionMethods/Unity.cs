using System;
using UnityEngine.Networking;

namespace J
{
	using UniRx;
	using UnityEngine;

	public static partial class ExtensionMethods
	{
		public static T GetOrAddComponent<T>(this GameObject @this) where T : Component
		{
			T component = @this.GetComponent<T>();
			if (component == null) component = @this.AddComponent<T>();
			return component;
		}

		public static T GetOrAddComponent<T>(this Component @this) where T : Component
		{
			return @this.gameObject.GetOrAddComponent<T>();
		}

		public static IObservable<AssetBundle> AsAssetBundleObservable(this UnityWebRequest @this)
		{
			return Observable.Defer(() => @this.Send().AsObservable()
				.Select(_ =>
				{
					try
					{
						return DownloadHandlerAssetBundle.GetContent(@this);
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
