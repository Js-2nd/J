using System;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

public static partial class GlobalExtensionMethods
{
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		T component = gameObject.GetComponent<T>();
		if (component == null) component = gameObject.AddComponent<T>();
		return component;
	}

	public static T GetOrAddComponent<T>(this Component component) where T : Component => component.gameObject.GetOrAddComponent<T>();

	public static IObservable<AssetBundle> AsAssetBundleObservable(this UnityWebRequest request, IProgress<float> progress = null)
	{
		return Observable.Defer(() => request.SendWebRequest().AsObservable(progress)
			.Select(_ =>
			{
				try
				{
					AssetBundle ab = DownloadHandlerAssetBundle.GetContent(request);
					if (ab == null) throw new Exception("Invalid AssetBundle");
					return ab;
				}
				catch
				{
					Debug.LogErrorFormat("AssetBundle not found. {0}", request.url);
					throw;
				}
			})
			.Finally(() => request.Dispose()));
	}
}
