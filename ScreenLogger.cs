namespace J
{
	using System.Collections.Generic;
	using UnityEngine;

	public class ScreenLogger : SingletonMonoBehaviour<ScreenLogger>
	{
		public float LogExpiration = 10;
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
			var log = new Log(++logIdCount,
				message, stackTrace, type,
				Time.realtimeSinceStartup + LogExpiration);
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

		void OnGUI() // TODO
		{
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

		public class Log
		{
			public readonly int Id;
			public readonly string Message;
			public readonly string StackTrace;
			public readonly LogType Type;
			public readonly float Expiration;

			public Log(int id, string message, string stackTrace, LogType type, float expiration)
			{
				Id = id;
				Message = $"<{Id}> {message}";
				StackTrace = !string.IsNullOrWhiteSpace(stackTrace) ? stackTrace : null;
				Type = type;
				Expiration = expiration;
			}
		}
	}
}
