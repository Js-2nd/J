namespace J
{
	using J.Internal;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UniRx;
	using UnityEditor;
	using UnityEngine;
	using Dict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>;

	[PreferBinarySerialization]
	public partial class UsageDatabase : ScriptableObject, ISerializationCallbackReceiver
	{
		public bool LogUpdate;
		public bool LogChangedFiles;
		public double SaveDelay = 2;

		[SerializeField, HideInInspector] List<Item> Data = new List<Item>();
		readonly Dict ReferDict = new Dict();
		readonly Dict DependDict = new Dict();

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
					AddPair(item.Id, dependId);
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

		void AddPair(string referId, string dependId)
		{
			ReferDict.GetOrAdd(dependId, _ => new HashSet<string>()).Add(referId);
			DependDict.GetOrAdd(referId, _ => new HashSet<string>()).Add(dependId);
		}

		void RemoveRefer(string id)
		{
			var dependIds = DependDict.GetOrDefault(id);
			if (dependIds == null) return;
			DependDict.Remove(id);
			foreach (string dependId in dependIds)
			{
				var referIds = ReferDict.GetOrDefault(dependId);
				if (referIds == null) continue;
				referIds.Remove(id);
				if (referIds.Count == 0) ReferDict.Remove(dependId);
			}
		}

		public IReadOnlyCollection<string> GetReferIds(string id) => ReferDict.GetOrDefault(id) ?? Empty;
		public IEnumerable<string> GetReferIds(IEnumerable<string> ids, int maxDepth = -1,
			SearchYieldType yieldType = SearchYieldType.ExcludeSourceAtFirst) =>
			Search.BreadthFirst(ids, GetReferIds, maxDepth, yieldType);

		public IReadOnlyCollection<string> GetDependIds(string id) => DependDict.GetOrDefault(id) ?? Empty;
		public IEnumerable<string> GetDependIds(IEnumerable<string> ids, int maxDepth = -1,
			SearchYieldType yieldType = SearchYieldType.ExcludeSourceAtFirst) =>
			Search.BreadthFirst(ids, GetDependIds, maxDepth, yieldType);

		string CountInfo => $"dep={DependDict.Count} ref={ReferDict.Count}";
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
			if (Init(true)) LogAssets(Instance.GetReferIds(Selection.assetGUIDs, 1), "reference", "references");
		}
		[MenuItem(MenuRoot + "Find References (Recursive)")]
		static void FindReferRecursive()
		{
			if (Init(true)) LogAssets(Instance.GetReferIds(Selection.assetGUIDs), "reference", "references");
		}

		[MenuItem(MenuRoot + "Find Dependencies")]
		static void FindDepend()
		{
			if (Init(true)) LogAssets(Instance.GetDependIds(Selection.assetGUIDs, 1), "dependency", "dependencies");
		}
		[MenuItem(MenuRoot + "Find Dependencies (Recursive)")]
		static void FindDependRecursive()
		{
			if (Init(true)) LogAssets(Instance.GetDependIds(Selection.assetGUIDs), "dependency", "dependencies");
		}

		static void LogAssets(IEnumerable<string> ids, string singular = null, string plural = null)
		{
			int count = 0;
			int deleted = 0;
			foreach (string id in ids)
				if (LogAsset(id)) count++;
				else deleted++;
			if (singular == null) return;
			if (plural == null) plural = singular;
			string suffix = deleted == 0 ? "." : $", {deleted} deleted.";
			switch (count)
			{
				case 0: Debug.Log($"No {plural} found{suffix}"); break;
				case 1: Debug.Log($"1 {singular} found{suffix}"); break;
				default: Debug.Log($"{count} {plural} found{suffix}"); break;
			}
		}

		static bool LogAsset(string id)
		{
			string path = AssetDatabase.GUIDToAssetPath(id);
			var asset = AssetDatabase.LoadMainAssetAtPath(path);
			if (asset == null)
			{
				Debug.LogWarning("[DELETED] " + path);
				return false;
			}
			Debug.Log($"[{asset.GetType().Name}] {path}", asset);
			return true;
		}

		[MenuItem(MenuRoot + "Refresh")]
		static void Create()
		{
			var db = Init();
			Instance = CreateInstance<UsageDatabase>();
			if (db)
			{
				Instance.LogUpdate = db.LogUpdate;
				Instance.LogChangedFiles = db.LogChangedFiles;
				Instance.SaveDelay = db.SaveDelay;
			}
			db = Instance;
			var paths = AssetDatabase.GetAllAssetPaths();
			for (int i = 0, iCount = paths.Length; i < iCount; i++)
			{
				if (ShowProgress("Creating " + ClassName, i, iCount, true))
				{
					DestroyImmediate(db);
					return;
				}
				db.AddRefer(paths[i]);
			}
			AssetDatabase.CreateAsset(db, DataPath);
			Debug.Log($"{ClassName} created. {db.CountInfo}", db);
		}

		public static bool ShowProgress(string title, int index, int count, bool cancelable = false)
		{
			if (index + 1 >= count)
			{
				EditorUtility.ClearProgressBar();
				return false;
			}
			if ((index & 63) == 0)
			{
				string info = $"{index}/{count}";
				float progress = (float)index / count;
				if (cancelable)
				{
					if (EditorUtility.DisplayCancelableProgressBar(title, info, progress))
					{
						EditorUtility.ClearProgressBar();
						return true;
					}
				}
				else
				{
					EditorUtility.DisplayProgressBar(title, info, progress);
				}
			}
			return false;
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
			static readonly HashSet<string> Changed = new HashSet<string>();
			static readonly SerialDisposable Saving = new SerialDisposable();

			static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
			{
				if (imported.Length == 0 && deleted.Length == 0) return;
				var db = Init();
				if (db == null) return;
				foreach (string path in deleted)
				{
					if (path == DataPath) continue;
					Changed.Add(path);
					db.RemoveRefer(AssetDatabase.AssetPathToGUID(path));
				}
				foreach (string path in imported)
				{
					if (path == DataPath) continue;
					Changed.Add(path);
					string id = AssetDatabase.AssetPathToGUID(path);
					db.RemoveRefer(id);
					db.AddRefer(path, id);
				}
				if (Changed.Count <= 0) return;
				try { Saving.Disposable = Observable.Timer(TimeSpan.FromSeconds(db.SaveDelay)).Subscribe(_ => Save()); }
				catch (InvalidOperationException) { } // MainThreadDispatcher.Awake.DontDestroyOnLoad
			}

			static void Save()
			{
				var db = Init();
				if (db == null) return;
				if (db.LogUpdate) Debug.Log($"{ClassName} updated. changed={Changed.Count} {db.CountInfo}", db);
				if (db.LogChangedFiles) foreach (string path in Changed) Debug.Log(path);
				Changed.Clear();
				EditorUtility.SetDirty(db);
			}
		}
	}
}
