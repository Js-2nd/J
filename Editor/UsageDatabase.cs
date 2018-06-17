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

	public partial class UsageDatabase : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField] List<Entry> Entries = new List<Entry>();
		Dictionary<string, HashSet<string>> RefToDepDict = new Dictionary<string, HashSet<string>>();
		Dictionary<string, HashSet<string>> DepToRefDict = new Dictionary<string, HashSet<string>>();

		IReadOnlyCollection<string> RefToDep(string refGUID) => RefToDepDict.GetOrDefault(refGUID) ?? Empty;
		IReadOnlyCollection<string> DepToRef(string depGUID) => DepToRefDict.GetOrDefault(depGUID) ?? Empty;

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			Entries.Clear();
			foreach (var item in RefToDepDict)
				if (item.Value.Count > 0)
					Entries.Add(new Entry(item.Key, item.Value.ToList()));
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			RefToDepDict.Clear();
			DepToRefDict.Clear();
			foreach (var entry in Entries)
			{
				string refGUID = entry.RefGUID;
				foreach (string depGUID in entry.DepGUIDs)
					AddPair(refGUID, depGUID);
			}
		}

		void AddRef(string refPath, string refGUID = null)
		{
			if (!refPath.StartsWith(AssetRoot)) return;
			if (refGUID == null) refGUID = AssetDatabase.AssetPathToGUID(refPath);
			foreach (string depPath in AssetDatabase.GetDependencies(refPath, false))
			{
				if (depPath == refPath || !depPath.StartsWith(AssetRoot)) continue;
				string depGUID = AssetDatabase.AssetPathToGUID(depPath);
				AddPair(refGUID, depGUID);
			}
		}

		void AddPair(string refGUID, string depGUID)
		{
			RefToDepDict.GetOrAdd(refGUID, _ => new HashSet<string>()).Add(depGUID);
			DepToRefDict.GetOrAdd(depGUID, _ => new HashSet<string>()).Add(refGUID);
		}

		void RemoveRef(string refGUID)
		{
			var depGUIDs = RefToDepDict.GetOrDefault(refGUID);
			if (depGUIDs == null) return;
			foreach (string depGUID in depGUIDs)
				DepToRefDict.GetOrDefault(depGUID)?.Remove(refGUID);
			depGUIDs.Clear();
		}

		void RemovePair(string refGUID, string depGUID)
		{
			RefToDepDict.GetOrDefault(refGUID)?.Remove(depGUID);
			DepToRefDict.GetOrDefault(depGUID)?.Remove(refGUID);
		}

		//IEnumerable<string> FindRef(IEnumerable<string> depGUIDs) => depGUIDs.SelectMany(DepToRef).Distinct();
		//IEnumerable<string> FindRefRecursive(IEnumerable<string> depGUIDs)
		//{
		//	var depSet = new HashSet<string>();
		//	var refSet = new HashSet<string>();
		//	var queue = new Queue<string>(depGUIDs);
		//	while (queue.Count > 0)
		//	{
		//		string depGUID = queue.Dequeue();
		//		if (depSet.Add(depGUID))
		//		{
		//			foreach (string refGUID in DepToRef(depGUID))
		//			{
		//			}
		//		}
		//	}
		//}
	}

	public partial class UsageDatabase
	{
		const string ClassName = nameof(UsageDatabase);
		const string MenuRoot = "Assets/Usage/";
		const string AssetRoot = "Assets/";

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
		static UsageDatabase Init(bool create)
		{
			if (Instance == null)
			{
				Load();
				if (Instance == null && create) Create();
			}
			return Instance;
		}

		[MenuItem(MenuRoot + "Find References")]
		static void FindRef()
		{
			if (!Init(true)) return;
			var refPaths = Selection.assetGUIDs.SelectMany(Instance.DepToRef)
				.Distinct().Select(AssetDatabase.GUIDToAssetPath);
			int count = 0;
			foreach (string refPath in refPaths)
			{
				++count;
				Debug.Log(refPath, AssetDatabase.LoadMainAssetAtPath(refPath));
			}
			switch (count)
			{
				case 0: Debug.Log("No references found."); break;
				case 1: Debug.Log("1 reference found."); break;
				default: Debug.Log($"{count} references found."); break;
			}
		}

		[MenuItem(MenuRoot + "Find Dependencies")]
		static void FindDep()
		{
			if (!Init(true)) return;
			var depPaths = Selection.assetGUIDs.SelectMany(Instance.RefToDep)
				.Distinct().Select(AssetDatabase.GUIDToAssetPath);
			int count = 0;
			foreach (string depPath in depPaths)
			{
				++count;
				Debug.Log(depPath, AssetDatabase.LoadMainAssetAtPath(depPath));
			}
			switch (count)
			{
				case 0: Debug.Log("No dependencies found."); break;
				case 1: Debug.Log("1 dependency found."); break;
				default: Debug.Log($"{count} dependencies found."); break;
			}
		}

		[MenuItem(MenuRoot + "Refresh")]
		static void Create()
		{
			Delete();
			Instance = CreateInstance<UsageDatabase>();
			var refPaths = AssetDatabase.GetAllAssetPaths();
			for (int i = 0, iCount = refPaths.Length; i < iCount; i++)
			{
				if (CancelableProgress("Creating " + ClassName, i + 1, iCount))
				{
					Delete();
					return;
				}
				Instance.AddRef(refPaths[i]);
			}
			AssetDatabase.CreateAsset(Instance, DataPath);
		}

		static void Load()
		{
			Instance = AssetDatabase.LoadAssetAtPath<UsageDatabase>(DataPath);
		}

		static void Delete()
		{
			if (Instance != null)
			{
				DestroyImmediate(Instance, true);
				Instance = null;
			}
			AssetDatabase.DeleteAsset(DataPath);
		}

		static void Dirty()
		{
			if (Instance != null) EditorUtility.SetDirty(Instance);
		}

		static bool CancelableProgress(string title, int nth, int count)
		{
			if (nth == count)
			{
				EditorUtility.ClearProgressBar();
				return false;
			}
			if ((nth & 63) == 1 && EditorUtility.DisplayCancelableProgressBar(title, $"{nth}/{count}", (float)nth / count))
			{
				EditorUtility.ClearProgressBar();
				return true;
			}
			return false;
		}

		[Serializable]
		class Entry
		{
			public string RefGUID;
			public List<string> DepGUIDs;

			public Entry() { }
			public Entry(string refGUID, List<string> depGUIDs)
			{
				RefGUID = refGUID;
				DepGUIDs = depGUIDs;
			}
		}

		class ModificationProcessor : AssetModificationProcessor
		{
			static string[] OnWillSaveAssets(string[] paths)
			{
				MainThreadDispatcher.Post(obj =>
				{
					var refPaths = obj as string[];
					if (refPaths == null) return;
					if (!Init(false))
					{
						Delete();
						return;
					}
					foreach (string refPath in refPaths)
					{
						string refGUID = AssetDatabase.AssetPathToGUID(refPath);
						Instance.RemoveRef(refGUID);
						Instance.AddRef(refPath, refGUID);
					}
					Dirty();
				}, paths);
				return paths;
			}

			static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
			{
				MainThreadDispatcher.Post(obj =>
				{
					string refPath = obj as string;
					if (refPath == null) return;
					if (!Init(false))
					{
						Delete();
						return;
					}
					Instance.RemoveRef(AssetDatabase.AssetPathToGUID(refPath));
					Dirty();
				}, path);
				return AssetDeleteResult.DidNotDelete;
			}
		}
	}
}
