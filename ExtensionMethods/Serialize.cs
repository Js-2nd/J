namespace J
{
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;

	public static partial class ExtensionMethods
	{
		public static byte[] SerializeToBytes(this object obj)
		{
			using (var s = new MemoryStream())
			{
				new BinaryFormatter().Serialize(s, obj);
				return s.ToArray();
			}
		}

		public static object DeserializeFromBytes(this byte[] bytes)
		{
			using (var s = new MemoryStream(bytes))
			{
				return new BinaryFormatter().Deserialize(s);
			}
		}

		public static T DeserializeFromBytes<T>(this byte[] bytes)
		{
			return (T)bytes.DeserializeFromBytes();
		}
	}
}
