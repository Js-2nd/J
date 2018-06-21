namespace J.Extensions
{
	using UnityEngine;

	public static partial class ExtensionMethods
	{
		public static T SetPosition<T>(this T tr, Vector3 pos) where T : Transform
		{
			tr.position = pos;
			return tr;
		}
		public static T SetPosition<T>(this T tr,
			float? x = null, float? y = null, float? z = null) where T : Transform
		{
			tr.position = tr.position.Overwrite(x, y, z);
			return tr;
		}

		public static T SetLocalPosition<T>(this T tr, Vector3 pos) where T : Transform
		{
			tr.localPosition = pos;
			return tr;
		}
		public static T SetLocalPosition<T>(this T tr,
			float? x = null, float? y = null, float? z = null) where T : Transform
		{
			tr.localPosition = tr.localPosition.Overwrite(x, y, z);
			return tr;
		}

		public static T SetRotation<T>(this T tr, Quaternion rot) where T : Transform
		{
			tr.rotation = rot;
			return tr;
		}

		public static T SetLocalRotation<T>(this T tr, Quaternion rot) where T : Transform
		{
			tr.localRotation = rot;
			return tr;
		}

		public static T SetEulerAngles<T>(this T tr, Vector3 angles) where T : Transform
		{
			tr.eulerAngles = angles;
			return tr;
		}
		public static T SetEulerAngles<T>(this T tr,
			float? x = null, float? y = null, float? z = null) where T : Transform
		{
			tr.eulerAngles = tr.eulerAngles.Overwrite(x, y, z);
			return tr;
		}

		public static T SetLocalEulerAngles<T>(this T tr, Vector3 angles) where T : Transform
		{
			tr.localEulerAngles = angles;
			return tr;
		}
		public static T SetLocalEulerAngles<T>(this T tr,
			float? x = null, float? y = null, float? z = null) where T : Transform
		{
			tr.localEulerAngles = tr.localEulerAngles.Overwrite(x, y, z);
			return tr;
		}

		public static T SetLocalScale<T>(this T tr, Vector3 scale) where T : Transform
		{
			tr.localScale = scale;
			return tr;
		}
		public static T SetLocalScale<T>(this T tr,
			float? x = null, float? y = null, float? z = null) where T : Transform
		{
			tr.localScale = tr.localScale.Overwrite(x, y, z);
			return tr;
		}
	}
}
