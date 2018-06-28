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
		public static ulong? GetContentLengthNum(this UnityWebRequest request)
		{
			ulong length;
			if (ulong.TryParse(request.GetContentLength(), out length)) return length;
			return null;
		}
	}
}
