namespace J
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	public class UsageDatabase : ScriptableObject
	{
		const string ClassName = nameof(UsageDatabase);
		const string AssetPath = "ProjectSettings/" + ClassName + ".asset";

		static UsageDatabase Instance;

		[MenuItem("Assets/Load Usage")]
		public static UsageDatabase Load()
		{
			if (Instance == null)
			{
				Instance = CreateInstance<UsageDatabase>();
				if (File.Exists(AssetPath))
				{
					EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(AssetPath), Instance);
					if (!Instance.OnLoad()) // TODO
					{
						Instance = null;
					}
				}
				else
				{
					if (!Instance.OnCreate()) // TODO
					{
						Instance = null;
					}
					else
					{
						File.WriteAllText(AssetPath, EditorJsonUtility.ToJson(Instance));
					}
				}
			}
			return Instance;
		}

		[SerializeField] List<Entry> Entries;

		Dictionary<string, HashSet<string>> Dependencies;
		Dictionary<string, HashSet<string>> References;

		bool OnCreate()
		{
			Dependencies = new Dictionary<string, HashSet<string>>();
			References = new Dictionary<string, HashSet<string>>();
			var allPaths = AssetDatabase.GetAllAssetPaths();
			for (int i = 0, iCount = allPaths.Length; i < iCount; i++)
			{
				if (CancelableProgress("Creating " + ClassName, i + 1, iCount)) return false;
				string refPath = allPaths[i];
				if (!refPath.StartsWith("Assets/")) continue;
				string refGUID = AssetDatabase.AssetPathToGUID(refPath);
				foreach (string depPath in AssetDatabase.GetDependencies(refPath, false))
				{
					if (depPath == refPath || !depPath.StartsWith("Assets/")) continue;
					string depGUID = AssetDatabase.AssetPathToGUID(depPath);
					Add(refGUID, depGUID);
				}
			}
			return WriteEntries();
		}

		bool OnLoad()
		{
			Dependencies = new Dictionary<string, HashSet<string>>();
			References = new Dictionary<string, HashSet<string>>();
			for (int i = 0, iCount = Entries.Count; i < iCount; i++)
			{
				if (CancelableProgress("Loading " + ClassName, i + 1, iCount)) return false;
				string refer = Entries[i].Reference;
				var depend = Entries[i].Dependencies;
				for (int j = 0, jCount = depend.Count; j < jCount; j++)
					Add(refer, depend[j]);
			}
			return true;
		}

		bool WriteEntries()
		{
			if (Entries != null) Entries.Clear();
			else Entries = new List<Entry>();
			int i = 0, iCount = Dependencies.Count;
			foreach (var item in Dependencies)
			{
				if (CancelableProgress("Writing " + ClassName, i + 1, iCount)) return false;
				if (item.Value.Count > 0) Entries.Add(new Entry(item.Key, item.Value.ToList()));
				i++;
			}
			return true;
		}

		void Add(string reference, string dependency)
		{
			Dependencies.GetOrAdd(reference, _ => new HashSet<string>()).Add(dependency);
			References.GetOrAdd(dependency, _ => new HashSet<string>()).Add(reference);
		}

		void Remove(string reference, string dependency)
		{
			Dependencies.GetOrDefault(reference)?.Remove(dependency);
			References.GetOrDefault(dependency)?.Remove(reference);
		}

		static bool CancelableProgress(string title, int nth, int count)
		{
			if (nth == count) EditorUtility.ClearProgressBar();
			else if ((nth & 31) == 1 && EditorUtility.DisplayCancelableProgressBar(title, $"{nth}/{count}", (float)nth / count))
			{
				EditorUtility.ClearProgressBar();
				return true;
			}
			return false;
		}

		[Serializable]
		public class Entry
		{
			public string Reference;
			public List<string> Dependencies;

			public Entry() { }
			public Entry(string reference, List<string> dependencies)
			{
				Reference = reference;
				Dependencies = dependencies;
			}
		}
	}
}
