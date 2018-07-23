namespace J
{
	using J.Internal;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	[PreferBinarySerialization]
	public partial class UsageDatabase : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField, HideInInspector] List<Item> Data = new List<Item>();
		Dictionary<string, HashSet<string>> ReferDict = new Dictionary<string, HashSet<string>>();
		Dictionary<string, HashSet<string>> DependDict = new Dictionary<string, HashSet<string>>();

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			Data.Clear();
			foreach (var item in DependDict)
				if (item.Value.Count > 0)
					Data.Add(new Item(item.Key, item.Value.ToList()));
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			ReferDict.Clear();
			DependDict.Clear();
			foreach (var item in Data)
				foreach (string dependId in item.DependIds)
					AddPair(item.Id, dependId, false);
		}

		void AddRefer(string path, string id = null)
		{
			if (!path.StartsWith("Assets/")) return;
			if (id == null) id = AssetDatabase.AssetPathToGUID(path);
			foreach (string dependPath in AssetDatabase.GetDependencies(path, false))
			{
				if (dependPath == path || !dependPath.StartsWith("Assets/")) continue;
				AddPair(id, AssetDatabase.AssetPathToGUID(dependPath));
			}
		}

		void AddPair(string referId, string dependId, bool setDirty = true)
		{
			ReferDict.GetOrAdd(dependId, _ => new HashSet<string>()).Add(referId);
			DependDict.GetOrAdd(referId, _ => new HashSet<string>()).Add(dependId);
			if (setDirty) EditorUtility.SetDirty(this);
		}

		void RemoveRefer(string id)
		{
			var dependIds = DependDict.GetOrDefault(id);
			if (dependIds == null) return;
			foreach (string dependId in dependIds)
				ReferDict.GetOrDefault(dependId)?.Remove(id);
			dependIds.Clear();
			EditorUtility.SetDirty(this);
		}

		public IReadOnlyCollection<string> GetReferIds(string id) => ReferDict.GetOrDefault(id) ?? Empty;
		public IEnumerable<string> GetReferIds(IEnumerable<string> ids, bool recursive = false) =>
			recursive ? BreadthFirstSearch(ids, GetReferIds) : ids.SelectMany(GetReferIds).Distinct();

		public IReadOnlyCollection<string> GetDependIds(string id) => DependDict.GetOrDefault(id) ?? Empty;
		public IEnumerable<string> GetDependIds(IEnumerable<string> ids, bool recursive = false) =>
			recursive ? BreadthFirstSearch(ids, GetDependIds) : ids.SelectMany(GetDependIds).Distinct();
	}

	partial class UsageDatabase
	{
		const string ClassName = nameof(UsageDatabase);
		const string MenuRoot = "Assets/" + ClassName + "/";

		static readonly IReadOnlyCollection<string> Empty = new string[0].AsReadOnly();

		public static readonly string DataPath = GetDataPath(true);
		static string GetDataPath(bool relative = false)
		{
			string dataPath = Path.ChangeExtension(CallerInfo.FilePath(), "asset");
			if (relative)
			{
				string cwd = Directory.GetCurrentDirectory();
				cwd = cwd.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				cwd += Path.DirectorySeparatorChar;
				dataPath = new Uri(cwd).MakeRelativeUri(new Uri(dataPath)).ToString();
			}
			return dataPath;
		}

		static UsageDatabase Instance;
		public static UsageDatabase Init(bool create = false)
		{
			if (Instance == null)
			{
				Instance = AssetDatabase.LoadAssetAtPath<UsageDatabase>(DataPath);
				if (Instance == null && create) Create();
			}
			return Instance;
		}

		[MenuItem(MenuRoot + "Find References")]
		static void FindRefer()
		{
			if (Init(true)) LogAssets(Instance.GetReferIds(Selection.assetGUIDs), "reference", "references");
		}
		[MenuItem(MenuRoot + "Find References (Recursive)")]
		static void FindReferRecursive()
		{
			if (Init(true)) LogAssets(Instance.GetReferIds(Selection.assetGUIDs, true), "reference", "references");
		}

		[MenuItem(MenuRoot + "Find Dependencies")]
		static void FindDepend()
		{
			if (Init(true)) LogAssets(Instance.GetDependIds(Selection.assetGUIDs), "dependency", "dependencies");
		}
		[MenuItem(MenuRoot + "Find Dependencies (Recursive)")]
		static void FindDependRecursive()
		{
			if (Init(true)) LogAssets(Instance.GetDependIds(Selection.assetGUIDs, true), "dependency", "dependencies");
		}

		static void LogAssets(IEnumerable<string> ids, string singular = null, string plural = null)
		{
			int count = 0;
			foreach (string id in ids)
			{
				count++;
				LogAsset(id);
			}
			if (singular == null) return;
			if (plural == null) plural = singular;
			switch (count)
			{
				case 0: Debug.Log($"No {plural} found."); break;
				case 1: Debug.Log($"1 {singular} found."); break;
				default: Debug.Log($"{count} {plural} found."); break;
			}
		}

		static void LogAsset(string id)
		{
			string path = AssetDatabase.GUIDToAssetPath(id);
			var asset = AssetDatabase.LoadMainAssetAtPath(path);
			Debug.Log($"[{asset.GetType().Name}] {path}", asset);
		}

		[MenuItem(MenuRoot + "Refresh")]
		static void Create()
		{
			Instance = CreateInstance<UsageDatabase>();
			var paths = AssetDatabase.GetAllAssetPaths();
			for (int i = 0, iCount = paths.Length; i < iCount; i++)
			{
				if (ShowProgress("Creating " + ClassName, i + 1, iCount, true))
				{
					DestroyImmediate(Instance);
					return;
				}
				Instance.AddRefer(paths[i]);
			}
			AssetDatabase.CreateAsset(Instance, DataPath);
			Debug.Log(ClassName + " created.", Instance);
		}

		static bool ShowProgress(string title, int nth, int count, bool cancelable = false)
		{
			if (nth == count)
			{
				EditorUtility.ClearProgressBar();
				return false;
			}
			if ((nth & 63) == 1)
			{
				if (cancelable)
				{
					if (EditorUtility.DisplayCancelableProgressBar(title, $"{nth}/{count}", (float)nth / count))
					{
						EditorUtility.ClearProgressBar();
						return true;
					}
				}
				else
				{
					EditorUtility.DisplayProgressBar(title, $"{nth}/{count}", (float)nth / count);
				}
			}
			return false;
		}

		static IEnumerable<T> BreadthFirstSearch<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> expander)
		{
			var visit = new HashSet<T>(source);
			var queue = new Queue<T>(visit);
			for (int skip = queue.Count; queue.Count > 0;)
			{
				var current = queue.Dequeue();
				if (--skip < 0) yield return current;
				foreach (var next in expander(current))
					if (visit.Add(next)) queue.Enqueue(next);
			}
		}

		[Serializable]
		class Item
		{
			public string Id;
			public List<string> DependIds;

			public Item() { }
			public Item(string id, List<string> dependIds)
			{
				Id = id;
				DependIds = dependIds;
			}
		}

		class Postprocessor : AssetPostprocessor
		{
			static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
			{
				if (!Init()) return;
				foreach (string path in deleted)
					Instance.RemoveRefer(AssetDatabase.AssetPathToGUID(path));
				foreach (string path in imported)
				{
					string id = AssetDatabase.AssetPathToGUID(path);
					Instance.RemoveRefer(id);
					Instance.AddRefer(path, id);
				}
			}
		}
	}
}
