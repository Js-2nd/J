namespace J
{
	public static partial class ExtensionMethods
	{
		public static bool HasFlag(this int @this, int flag)
		{
			return (@this & flag) == flag;
		}

		public static int SetFlag(this int @this, int flag)
		{
			return @this | flag;
		}

		public static int UnsetFlag(this int @this, int flag)
		{
			return @this & ~flag;
		}

		public static int FlipFlag(this int @this, int flag)
		{
			return @this ^ flag;
		}
	}
}
