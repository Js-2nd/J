namespace J
{
	using UnityEngine;

	public class GUIDrag
	{
		public Rect Rect;
		public bool Pressing;
		public bool Dragged;
		public Vector2 MouseOffset;

		public GUIDrag() { }
		public GUIDrag(float x, float y, float width, float height)
		{
			Rect = new Rect(x, y, width, height);
		}
		public GUIDrag(Vector2 position, Vector2 size)
		{
			Rect = new Rect(position, size);
		}
		public GUIDrag(Rect rect)
		{
			Rect = rect;
		}

		public void OnGUI()
		{
			var e = Event.current;
			switch (e.type)
			{
				case EventType.MouseDown:
					if (Rect.Contains(e.mousePosition))
					{
						Pressing = true;
						Dragged = false;
						MouseOffset = e.mousePosition - Rect.position;
					}
					break;
				case EventType.MouseDrag:
					if (Pressing)
					{
						Dragged = true;
						Rect.position = e.mousePosition - MouseOffset;
						var min = GUIUtility.ScreenToGUIPoint(Vector2.zero);
						var max = GUIUtility.ScreenToGUIPoint(new Vector2(Screen.width, Screen.height));
						if (Rect.xMin < min.x) Rect.x = min.x;
						else if (Rect.xMax > max.x) Rect.x = max.x - Rect.width;
						if (Rect.yMin < min.y) Rect.y = min.y;
						else if (Rect.yMax > max.y) Rect.y = max.y - Rect.height;
					}
					break;
				case EventType.MouseUp:
					if (Pressing) Pressing = false;
					break;
			}
		}

		public static implicit operator Rect(GUIDrag drag) => drag.Rect;

		public static implicit operator bool(GUIDrag drag) => !drag.Dragged;
	}
}
