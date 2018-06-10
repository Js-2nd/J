namespace J
{
	using System.Collections.Generic;
	using UnityEngine;

	public class ScreenLogger : SingletonMonoBehaviour<ScreenLogger>
	{
		public int LogCapacity = 200;
		public float AutoHideTime = 10; // TODO
		public bool Expand; // TODO

		LinkedList<Log> list;
		Dictionary<int, LinkedListNode<Log>> dict;
		GUIDrag drag;

		protected override void SingletonAwake()
		{
			base.SingletonAwake();
			list = new LinkedList<Log>();
			dict = new Dictionary<int, LinkedListNode<Log>>();
			drag = new GUIDrag(0, 0, Screen.width * 0.5f, Screen.height * 0.5f); // TODO
			Application.logMessageReceived += OnLogReceived;
		}

		protected override void SingletonOnDestroy()
		{
			Application.logMessageReceived -= OnLogReceived;
			base.SingletonOnDestroy();
		}

		int logIdCount;
		void OnLogReceived(string message, string stackTrace, LogType type)
		{
			var log = new Log(++logIdCount, message, stackTrace, type);
			var node = list.AddLast(log);
			dict.Add(log.Id, node);
			if (list.Count > LogCapacity)
			{
				dict.Remove(list.First.Value.Id);
				list.RemoveFirst();
			}
		}

		public void Clear()
		{
			list.Clear();
			dict.Clear();
		}

		int current;
		void OnGUI() // TODO
		{
			var rect = drag.Rect;
			rect.xMax -= 20;
			if (list.Count > 0)
			{
				current = (int)(GUI.VerticalScrollbar(new Rect(rect.xMax, rect.yMin, 20, rect.height), current, 5, list.First.Value.Id, list.Last.Value.Id) + 0.5f);
			}
			GUILayout.BeginArea(rect, string.Empty, "box");
			for (var node = dict.GetOrDefault(current, list.First); node != null; node = node.Next)
			{
				var log = node.Value;
				if (log.StackTrace == null)
				{
					GUILayout.Label(log.Message);
				}
				else
				{
					log.ShowStackTrace = GUILayout.Toggle(log.ShowStackTrace, log.Message);
					if (log.ShowStackTrace) GUILayout.Label(log.StackTrace);
				}
				if (node == list.Last) break;
			}
			GUILayout.EndArea();
			drag.OnGUI();
		}

		public class Log
		{
			public readonly int Id;
			public readonly string Message;
			public readonly string StackTrace;
			public readonly LogType Type;
			public readonly float Timestamp;
			public bool ShowStackTrace;

			public Log(int id, string message, string stackTrace, LogType type)
			{
				Id = id;
				Message = $"<{Id}> {message}";
				StackTrace = !string.IsNullOrWhiteSpace(stackTrace) ? stackTrace : null;
				Type = type;
				Timestamp = Time.realtimeSinceStartup;
			}
		}
	}
}
