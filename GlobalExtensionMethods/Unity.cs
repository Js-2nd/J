public static partial class GlobalExtensionMethods
{
	public static T GetOrAddComponent<T>(this UnityEngine.GameObject gameObject) where T : UnityEngine.Component
	{
		T component = gameObject.GetComponent<T>();
		if (component == null) component = gameObject.AddComponent<T>();
		return component;
	}

	public static T GetOrAddComponent<T>(this UnityEngine.Component component) where T : UnityEngine.Component => component.gameObject.GetOrAddComponent<T>();
}
