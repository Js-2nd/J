namespace J
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	public class ScreenLogger : SingletonMonoBehaviour<ScreenLogger>
	{
		public float LogExpire = 10;
		public int LogCapacity = 200;

		LinkedList<Log> list;
		Dictionary<int, LinkedListNode<Log>> dict;

		protected override void SingletonAwake()
		{
			base.SingletonAwake();
			list = new LinkedList<Log>();
			dict = new Dictionary<int, LinkedListNode<Log>>();
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
			var expire = LogExpire > 0 ? Time.realtimeSinceStartup + LogExpire : (float?)null;
			var log = new Log(++logIdCount, message, stackTrace, type, expire);
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

		void Update() // TODO
		{
			if (Input.GetMouseButtonDown(0)) Debug.Log("This is a log message.");
			if (Input.GetMouseButtonDown(1)) Debug.LogWarning("This is a warning message.");
			if (Input.GetMouseButtonDown(2)) Debug.LogError("This is an error message.");
		}

		LinkedListNode<Log> startNode;
		void OnGUI() // TODO
		{
			if (list.Count <= 0) return;
			ScaleGUI();
			using (new State())
			{
				if (startNode?.List == null) startNode = list.First;
				while (startNode?.Value.Expire < Time.realtimeSinceStartup)
					startNode = startNode.Next;
				for (var node = startNode; node != null; node = node.Next)
				{
					var log = node.Value;
					GUI.contentColor = LogTypeColor(log.Type);
					GUILayout.Box(log.Message);
				}
			}
		}

		static void ScaleGUI()
		{
			float scale = Mathf.Max(Screen.width, Screen.height) / 1920f;
			scale += Mathf.Min(Screen.width, Screen.height) / 1080f;
			GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);
		}

		static Color LogTypeColor(LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Assert:
				case LogType.Exception: return Color.red;
				case LogType.Warning: return Color.yellow;
				default: return Color.white;
			}
		}

		class Log
		{
			public readonly int Id;
			public readonly string Message;
			public readonly string StackTrace;
			public readonly LogType Type;
			public readonly float? Expire;

			public Log(int id, string message, string stackTrace, LogType type, float? expire)
			{
				Id = id;
				Message = $"<{Id}> {message}";
				StackTrace = !string.IsNullOrWhiteSpace(stackTrace) ? stackTrace : null;
				Type = type;
				Expire = expire;
			}
		}

		class State : IDisposable
		{
			readonly Color contentColor;

			public State()
			{
				contentColor = GUI.contentColor;
			}

			public void Dispose()
			{
				GUI.contentColor = contentColor;
			}
		}
	}
}
