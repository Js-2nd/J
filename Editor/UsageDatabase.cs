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

	public static class UsageDatabase
	{
		const string ClassName = nameof(UsageDatabase);
		const string AssetRoot = "Assets/";

		public static readonly string DataPath = GetDataPath(true);
		static string GetDataPath(bool relative = false)
		{
			string dataPath = Path.ChangeExtension(CallerInfo.FilePath(), "bytes");
			if (relative)
			{
				string cwd = Directory.GetCurrentDirectory();
				cwd = cwd.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				cwd += Path.DirectorySeparatorChar;
				dataPath = new Uri(cwd).MakeRelativeUri(new Uri(dataPath)).ToString();
			}
			return dataPath;
		}

		static bool Initialized;
		static bool Init()
		{
			if (Initialized) return true;
			bool hasData = File.Exists(DataPath);
			return Initialized = !hasData && Create() && Save() || hasData && Load();
		}

		[MenuItem("Assets/Refresh " + ClassName)]
		static bool ForceInit() => Initialized = Create() && Save();

		static readonly IReadOnlyCollection<string> Empty = new string[0].AsReadOnly();

		static readonly Dictionary<string, HashSet<string>> RefToDep = new Dictionary<string, HashSet<string>>();
		static IReadOnlyCollection<string> GetDep(string refGUID) => RefToDep.GetOrDefault(refGUID) ?? Empty;

		static readonly Dictionary<string, HashSet<string>> DepToRef = new Dictionary<string, HashSet<string>>();
		static IReadOnlyCollection<string> GetRef(string depGUID) => DepToRef.GetOrDefault(depGUID) ?? Empty;

		static bool Create()
		{
			RefToDep.Clear();
			DepToRef.Clear();
			var refPaths = AssetDatabase.GetAllAssetPaths();
			for (int i = 0, iCount = refPaths.Length; i < iCount; i++)
			{
				if (CancelableProgress("Creating " + ClassName, i + 1, iCount)) return false;
				AddRef(refPaths[i]);
			}
			return true;
		}

		static bool Save()
		{
			var data = new Data();
			var list = data.L;
			int i = 0, iCount = RefToDep.Count;
			foreach (var item in RefToDep)
			{
				if (CancelableProgress("Writing " + ClassName, i + 1, iCount)) return false;
				if (item.Value.Count > 0) list.Add(new Entry(item.Key, item.Value.ToList()));
				i++;
			}
			File.WriteAllText(DataPath, EditorJsonUtility.ToJson(data));
			return true;
		}

		static bool Load()
		{
			var data = new Data();
			EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(DataPath), data);
			var list = data.L;
			RefToDep.Clear();
			DepToRef.Clear();
			for (int i = 0, iCount = list.Count; i < iCount; i++)
			{
				if (CancelableProgress("Loading " + ClassName, i + 1, iCount)) return false;
				string refGUID = list[i].R;
				var depGUIDs = list[i].D;
				for (int j = 0, jCount = depGUIDs.Count; j < jCount; j++) AddPair(refGUID, depGUIDs[j]);
			}
			return true;
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

		static void AddRef(string refPath, string refGUID = null)
		{
			if (!refPath.StartsWith(AssetRoot)) return;
			if (refGUID == null) refGUID = AssetDatabase.AssetPathToGUID(refPath);
			var depPaths = AssetDatabase.GetDependencies(refPath, false);
			for (int i = 0, iCount = depPaths.Length; i < iCount; i++)
			{
				string depPath = depPaths[i];
				if (depPath == refPath || !depPath.StartsWith(AssetRoot)) continue;
				string depGUID = AssetDatabase.AssetPathToGUID(depPath);
				AddPair(refGUID, depGUID);
			}
		}

		static void AddPair(string refGUID, string depGUID)
		{
			RefToDep.GetOrAdd(refGUID, _ => new HashSet<string>()).Add(depGUID);
			DepToRef.GetOrAdd(depGUID, _ => new HashSet<string>()).Add(refGUID);
		}

		static void RemoveRef(string refGUID)
		{
			var depGUIDs = RefToDep.GetOrDefault(refGUID);
			if (depGUIDs == null) return;
			foreach (string depGUID in depGUIDs)
				DepToRef.GetOrDefault(depGUID)?.Remove(refGUID);
			depGUIDs.Clear();
		}

		static void RemovePair(string refGUID, string depGUID)
		{
			RefToDep.GetOrDefault(refGUID)?.Remove(depGUID);
			DepToRef.GetOrDefault(depGUID)?.Remove(refGUID);
		}

		[MenuItem("Assets/Find Usages")]
		static void FindUsages()
		{
			if (!Init()) return;
			var refPaths = Selection.assetGUIDs.SelectMany(GetRef)
				.Distinct().Select(AssetDatabase.GUIDToAssetPath);
			foreach (string refPath in refPaths)
				Debug.Log(refPath, AssetDatabase.LoadMainAssetAtPath(refPath));
		}

		[Serializable]
		internal class Data
		{
			public List<Entry> L = new List<Entry>();
		}

		[Serializable]
		internal class Entry
		{
			public string R;
			public List<string> D;

			public Entry() { }
			public Entry(string refGUID, List<string> depGUIDs)
			{
				R = refGUID;
				D = depGUIDs;
			}
		}

		class ModificationProcessor : AssetModificationProcessor
		{
			static string[] OnWillSaveAssets(string[] paths)
			{
				MainThreadDispatcher.Post(obj =>
				{
					var refPaths = obj as string[];
					if (refPaths == null || !Initialized) return;
					for (int i = 0, iCount = refPaths.Length; i < iCount; i++)
					{
						string refPath = refPaths[i];
						string refGUID = AssetDatabase.AssetPathToGUID(refPath);
						Debug.Log($"{refPath} {refGUID} {AssetDatabase.GetDependencies(refPath, false).Length}");
						RemoveRef(refGUID);
						AddRef(refPath, refGUID);
					}
					if (!Save()) Initialized = false;
				}, paths);
				return paths;
			}

			static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
			{
				MainThreadDispatcher.Post(obj =>
				{
					string path = obj as string;
				}, assetPath);
				return AssetDeleteResult.DidNotDelete;
			}
		}
	}
}
