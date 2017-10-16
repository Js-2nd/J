namespace J
{
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;

	public static partial class ExtensionMethods
	{
		public static byte[] SerializeToBytes(this object @this)
		{
			using (var s = new MemoryStream())
			{
				new BinaryFormatter().Serialize(s, @this);
				return s.ToArray();
			}
		}

		public static object DeserializeFromBytes(this byte[] @this)
		{
			using (var s = new MemoryStream(@this))
			{
				return new BinaryFormatter().Deserialize(s);
			}
		}

		public static T DeserializeFromBytes<T>(this byte[] @this)
		{
			return (T)@this.DeserializeFromBytes();
		}
	}
}
