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
						if (Rect.xMin < 0) Rect.x = 0;
						else if (Rect.xMax > Screen.width) Rect.x = Screen.width - Rect.width;
						if (Rect.yMin < 0) Rect.y = 0;
						else if (Rect.yMax > Screen.height) Rect.y = Screen.height - Rect.height;
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
