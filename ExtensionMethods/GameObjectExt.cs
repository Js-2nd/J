using UnityEngine;

namespace J
{
	public static partial class ExtensionMethods
	{
		public static T GetOrAddComponent<T>(this GameObject @this) where T : Component
		{
			T component = @this.GetComponent<T>();
			if (component == null) component = @this.AddComponent<T>();
			return component;
		}
	}
}
