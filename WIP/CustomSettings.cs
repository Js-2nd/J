namespace J
{
#if UNITY_EDITOR
	using System.IO;
	using UnityEditor;
#endif
	using UnityEngine;

	public abstract class CustomSettings<T> : ScriptableObject where T : CustomSettings<T>
	{
		const string SettingsDirectory = "Assets/Resources";

		static T Instance;

		public static T Load()
		{
			if (Instance == null)
			{
				var name = typeof(T).Name;
				Instance = Resources.Load<T>(name);
				if (Instance == null)
				{
					Instance = CreateInstance<T>();
#if UNITY_EDITOR
					if (!Directory.Exists(SettingsDirectory))
						Directory.CreateDirectory(SettingsDirectory);
					AssetDatabase.CreateAsset(Instance, string.Format("{0}/{1}.asset", SettingsDirectory, name));
#endif
				}
			}
			return Instance;
		}
	}
}
