namespace J.Internal
{
	using System.Runtime.CompilerServices;

	public static class CallerInfo
	{
		public static string FilePath([CallerFilePath] string _ = null) => _;
	}
}
