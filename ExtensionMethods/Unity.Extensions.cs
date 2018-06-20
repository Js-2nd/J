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
		public static T SetPosition<T>(this T tr, float? x = null,
			float? y = null, float? z = null) where T : Transform
		{
			var pos = tr.position;
			if (x.HasValue) pos.x = x.Value;
			if (y.HasValue) pos.y = y.Value;
			if (z.HasValue) pos.z = z.Value;
			tr.position = pos;
			return tr;
		}
		public static T SetLocalPosition<T>(this T tr, Vector3 pos) where T : Transform
		{
			tr.localPosition = pos;
			return tr;
		}
		public static T SetLocalPosition<T>(this T tr, float? x = null,
			float? y = null, float? z = null) where T : Transform
		{
			var pos = tr.localPosition;
			if (x.HasValue) pos.x = x.Value;
			if (y.HasValue) pos.y = y.Value;
			if (z.HasValue) pos.z = z.Value;
			tr.localPosition = pos;
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
		public static T SetEulerAngles<T>(this T tr, float? x = null, float? y = null, float? z = null) where T : Transform
		{
			var angles = tr.eulerAngles;
			if (x.HasValue) angles.x = x.Value;
			if (y.HasValue) angles.y = y.Value;
			if (z.HasValue) angles.z = z.Value;
			tr.eulerAngles = angles;
			return tr;
		}
		public static T SetLocalEulerAngles<T>(this T tr, Vector3 angles) where T : Transform
		{
			tr.localEulerAngles = angles;
			return tr;
		}
		public static T SetLocalEulerAngles<T>(this T tr, float? x = null, float? y = null, float? z = null) where T : Transform
		{
			var angles = tr.localEulerAngles;
			if (x.HasValue) angles.x = x.Value;
			if (y.HasValue) angles.y = y.Value;
			if (z.HasValue) angles.z = z.Value;
			tr.localEulerAngles = angles;
			return tr;
		}

		public static T SetLocalScale<T>(this T tr, Vector3 scale) where T : Transform
		{
			tr.localScale = scale;
			return tr;
		}
		public static T SetLocalScale<T>(this T tr, float? x = null, float? y = null, float? z = null) where T : Transform
		{
			var scale = tr.localScale;
			if (x.HasValue) scale.x = x.Value;
			if (y.HasValue) scale.y = y.Value;
			if (z.HasValue) scale.z = z.Value;
			tr.localScale = scale;
			return tr;
		}

		public static Canvas GetCanvas(this RectTransform rt) => rt.GetComponentInParent<Canvas>();

		public static Rect GetScreenRect(this RectTransform rt)
		{
			var min = rt.TransformPoint(rt.rect.min);
			var max = rt.TransformPoint(rt.rect.max);
			return new Rect(min, max - min);
		}

		public static RectTransform SetScreenRect(this RectTransform rt, Rect rect) =>
			rt.SetScreenRect(rect, rt.GetCanvas().rootCanvas.worldCamera);
		public static RectTransform SetScreenRect(this RectTransform rt, Rect rect, Camera cameraHint)
		{
			Vector3 min, max;
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, rect.min, cameraHint, out min) &&
				RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, rect.max, cameraHint, out max))
			{
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = Vector2.zero;
				rt.pivot = Vector2.zero;
				rt.sizeDelta = max - min;
				rt.position = min;
			}
			return rt;
		}
	}
}
