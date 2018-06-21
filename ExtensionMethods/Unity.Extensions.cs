namespace J.Extensions
{
	using UnityEngine;

	public static partial class ExtensionMethods
	{
		public static SpriteRenderer SetColor(this SpriteRenderer sr, Color color)
		{
			sr.color = color;
			return sr;
		}
		public static SpriteRenderer SetColor(this SpriteRenderer sr,
			float? r = null, float? g = null, float? b = null, float? a = null)
		{
			sr.color = sr.color.Overwrite(r, g, b, a);
			return sr;
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
