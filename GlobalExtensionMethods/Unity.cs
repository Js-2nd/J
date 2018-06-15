using UnityEngine;

public static partial class GlobalExtensionMethods
{
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		T component = gameObject.GetComponent<T>();
		if (component != null) return component;
		return gameObject.AddComponent<T>();
	}

	public static T GetOrAddComponent<T>(this Component component) where T : Component =>
		component.gameObject.GetOrAddComponent<T>();
}
