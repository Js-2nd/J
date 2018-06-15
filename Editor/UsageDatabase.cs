namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	public class UsageDatabase : ScriptableObject
	{
		const string ClassName = nameof(UsageDatabase);

		static UsageDatabase Instance;

		public static UsageDatabase Load()
		{
			if (Instance == null)
			{
				var temp = CreateInstance<UsageDatabase>();
				string path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(temp));
				path = path.Substring(0, path.Length - 2) + "asset";
				Instance = AssetDatabase.LoadAssetAtPath<UsageDatabase>(path);
				if (Instance == null)
				{
					Instance = temp;
					if (!Instance.OnCreate()) // TODO
					{
						Instance = null;
					}
					AssetDatabase.CreateAsset(Instance, path);
				}
				else
				{
					DestroyImmediate(temp);
					if (!Instance.OnLoad()) // TODO
					{
						Instance = null;
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
				const string title = "Creating " + ClassName;
				if ((i & 31) == 0 && DisplayProgress(title, i, iCount))
				{
					EditorUtility.ClearProgressBar();
					return false;
				}
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
				const string title = "Loading " + ClassName;
				if ((i & 31) == 0 && DisplayProgress(title, i, iCount))
				{
					EditorUtility.ClearProgressBar();
					return false;
				}
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
				const string title = "Writing " + ClassName;
				if ((i & 31) == 0 && DisplayProgress(title, i, iCount))
				{
					EditorUtility.ClearProgressBar();
					return false;
				}
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

		static bool DisplayProgress(string title, int i, int count)
		{
			return EditorUtility.DisplayCancelableProgressBar(title, $"{i + 1}/{count}", (float)i / count);
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
