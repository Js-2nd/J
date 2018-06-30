namespace J
{
	public interface IUnityWebRequestSendOptions
	{
		string ETag { get; }
		bool ThrowNetworkError { get; }
		bool ThrowHttpError { get; }
		bool AutoDispose { get; }
	}

	public class UnityWebRequestSendOptions : UnityWebRequestSendOptions<UnityWebRequestSendOptions> { }

	public class UnityWebRequestSendOptions<T> : IUnityWebRequestSendOptions where T : UnityWebRequestSendOptions<T>
	{
		public string ETag { get; set; }
		public bool ThrowNetworkError { get; set; } = true;
		public bool ThrowHttpError { get; set; } = true;
		public bool AutoDispose { get; set; } = true;

		public T SetETag(string eTag)
		{
			ETag = eTag;
			return (T)this;
		}
		public T SetThrowNetworkError(bool throwNetworkError)
		{
			ThrowNetworkError = throwNetworkError;
			return (T)this;
		}
		public T SetThrowHttpError(bool throwHttpError)
		{
			ThrowHttpError = throwHttpError;
			return (T)this;
		}
		public T SetAutoDispose(bool autoDispose)
		{
			AutoDispose = autoDispose;
			return (T)this;
		}
	}
}
