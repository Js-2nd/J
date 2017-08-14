using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace J
{
	public static partial class ExtensionMethods
	{
		public static string SerializeToBase64<T>(this T @this)
		{
			using (var s = new MemoryStream())
			{
				var fmt = new BinaryFormatter();
				fmt.Serialize(s, @this);
				return Convert.ToBase64String(s.ToArray());
			}
		}

		public static T DeserializeFromBase64<T>(this string @this)
		{
			using (var s = new MemoryStream(Convert.FromBase64String(@this)))
			{
				var fmt = new BinaryFormatter();
				return (T)fmt.Deserialize(s);
			}
		}
	}
}
