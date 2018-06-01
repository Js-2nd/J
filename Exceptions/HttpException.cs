namespace J
{
	using System;
	using UnityEngine.Networking;

	[Serializable]
	public class HttpException : Exception
	{
		public UnityWebRequest Request { get; }
		public long ResponseCode { get; }
		public string Method { get; }
		public string Url { get; }
		public override string Message { get; }

		public HttpException(UnityWebRequest request)
		{
			Request = request;
			ResponseCode = request.responseCode;
			Method = request.method;
			Url = request.url;
			Message = $"HTTP{ResponseCode} [{Method}] {Url}";
		}
	}
}
