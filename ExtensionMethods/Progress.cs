namespace J
{
	using System;
	using UnityEngine;

	public static partial class ExtensionMethods
	{
		public static void ReportWithoutException<T>(this IProgress<T> @this, T value, bool logError = true)
		{
			try
			{
				@this.Report(value);
			}
			catch (Exception ex)
			{
				if (logError)
				{
					Debug.LogError(ex);
				}
			}
		}
	}
}
