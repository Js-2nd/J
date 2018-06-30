namespace J
{
	using J.Internal;
	using UnityEngine.Networking;

	public static partial class ExtensionMethods
	{
		public static string GetETag(this UnityWebRequest request) =>
			request.GetResponseHeader(HttpHeader.ETag);

		public static string GetContentLength(this UnityWebRequest request) =>
			request.GetResponseHeader(HttpHeader.ContentLength);
		public static long? GetContentLengthNum(this UnityWebRequest request)
		{
			long length;
			if (long.TryParse(request.GetContentLength(), out length)) return length;
			return null;
		}
	}
}
