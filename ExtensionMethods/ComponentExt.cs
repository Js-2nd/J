using UnityEngine;

namespace J
{
	public static partial class ExtensionMethods
	{
		public static T GetOrAddComponent<T>(this Component @this) where T : Component
		{
			T component = @this.GetComponent<T>();
			if (component == null) component = @this.gameObject.AddComponent<T>();
			return component;
		}
	}
}
