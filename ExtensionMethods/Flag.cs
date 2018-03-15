namespace J
{
	public static partial class ExtensionMethods
	{
		public static bool HasFlag(this int value, int flag) => (value & flag) == flag;
		public static int SetFlag(this int value, int flag) => value | flag;
		public static int UnsetFlag(this int value, int flag) => value & ~flag;
		public static int FlipFlag(this int value, int flag) => value ^ flag;

		public static bool HasFlag(this long value, long flag) => (value & flag) == flag;
		public static long SetFlag(this long value, long flag) => value | flag;
		public static long UnsetFlag(this long value, long flag) => value & ~flag;
		public static long FlipFlag(this long value, long flag) => value ^ flag;
	}
}
