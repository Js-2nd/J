namespace J
{
	public static partial class ExtensionMethods
	{
		public static bool HasFlag(this int @this, int flag) => (@this & flag) == flag;
		public static int SetFlag(this int @this, int flag) => @this | flag;
		public static int UnsetFlag(this int @this, int flag) => @this & ~flag;
		public static int FlipFlag(this int @this, int flag) => @this ^ flag;

		public static bool HasFlag(this long @this, long flag) => (@this & flag) == flag;
		public static long SetFlag(this long @this, long flag) => @this | flag;
		public static long UnsetFlag(this long @this, long flag) => @this & ~flag;
		public static long FlipFlag(this long @this, long flag) => @this ^ flag;
	}
}
