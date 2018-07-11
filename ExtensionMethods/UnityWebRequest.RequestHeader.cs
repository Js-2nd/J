namespace J
{
	using J.Internal;
	using UnityEngine.Networking;

	public static partial class ExtensionMethods
	{
		public static UnityWebRequest SetIfModifiedSince(this UnityWebRequest request, string lastModified)
		{
			request.SetRequestHeader(HttpHeader.IfModifiedSince, lastModified);
			return request;
		}
		public static string GetIfModifiedSince(this UnityWebRequest request) =>
			request.GetRequestHeader(HttpHeader.IfModifiedSince);

		public static UnityWebRequest SetIfNoneMatch(this UnityWebRequest request, string eTag)
		{
			request.SetRequestHeader(HttpHeader.IfNoneMatch, eTag);
			return request;
		}
		public static string GetIfNoneMatch(this UnityWebRequest request) =>
			request.GetRequestHeader(HttpHeader.IfNoneMatch);
	}
}
