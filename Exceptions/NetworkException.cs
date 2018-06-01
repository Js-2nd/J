namespace J
{
	using System;
	using UnityEngine.Networking;

	[Serializable]
	public class NetworkException : Exception
	{
		public UnityWebRequest Request { get; }
		public string Error { get; }
		public string Method { get; }
		public string Url { get; }
		public override string Message { get; }

		public NetworkException(UnityWebRequest request)
		{
			Request = request;
			Error = request.error;
			Method = request.method;
			Url = request.url;
			Message = $"{Error} [{Method}] {Url}";
		}
	}
}
