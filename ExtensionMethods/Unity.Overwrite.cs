namespace J.Extensions
{
	using UnityEngine;

	public static partial class ExtensionMethods
	{
		public static Vector2 Overwrite(this Vector2 v,
			float? x = null, float? y = null)
		{
			if (x.HasValue) v.x = x.Value;
			if (y.HasValue) v.y = y.Value;
			return v;
		}

		public static Vector2Int Overwrite(this Vector2Int v,
			int? x = null, int? y = null)
		{
			if (x.HasValue) v.x = x.Value;
			if (y.HasValue) v.y = y.Value;
			return v;
		}

		public static Vector3 Overwrite(this Vector3 v,
			float? x = null, float? y = null, float? z = null)
		{
			if (x.HasValue) v.x = x.Value;
			if (y.HasValue) v.y = y.Value;
			if (z.HasValue) v.z = z.Value;
			return v;
		}

		public static Vector3Int Overwrite(this Vector3Int v,
			int? x = null, int? y = null, int? z = null)
		{
			if (x.HasValue) v.x = x.Value;
			if (y.HasValue) v.y = y.Value;
			if (z.HasValue) v.z = z.Value;
			return v;
		}

		public static Vector4 Overwrite(this Vector4 v,
			float? x = null, float? y = null, float? z = null, float? w = null)
		{
			if (x.HasValue) v.x = x.Value;
			if (y.HasValue) v.y = y.Value;
			if (z.HasValue) v.z = z.Value;
			if (w.HasValue) v.w = w.Value;
			return v;
		}

		public static Color Overwrite(this Color c,
			float? r = null, float? g = null, float? b = null, float? a = null)
		{
			if (r.HasValue) c.r = r.Value;
			if (g.HasValue) c.g = g.Value;
			if (b.HasValue) c.b = b.Value;
			if (a.HasValue) c.a = a.Value;
			return c;
		}
	}
}
