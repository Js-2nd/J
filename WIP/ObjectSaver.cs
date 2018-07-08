namespace J
{
	using System.IO;
	using System.Runtime.Serialization.Json;

	public static class ObjectSaver
	{
		public static void SaveToJsonFile<T>(T obj, string path, bool ensureDirectory = false)
		{
			if (ensureDirectory)
			{
				string dir = Path.GetDirectoryName(path);
				if (dir != null && !Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}
			using (var file = File.Open(path, FileMode.Create, FileAccess.Write))
			{
				new DataContractJsonSerializer(typeof(T)).WriteObject(file, obj);
			}
		}

		public static T LoadFromJsonFile<T>(string path)
		{
			using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
			{
				return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(file);
			}
		}

		public static bool TryLoadFromJsonFile<T>(string path, out T obj)
		{
			try
			{
				obj = LoadFromJsonFile<T>(path);
				return true;
			}
			catch
			{
				obj = default(T);
				return false;
			}
		}
	}
}
