namespace J
{
	using J.Internal;
	using System;
	using UniRx;

	public static partial class ExtensionMethods
	{
		public static IObservable<T> FakeProgress<T, T2>(this IObservable<T> source,
			IProgress<float> progress, IObservable<T2> reporter, double countHint, double progressHint = 0.8)
		{
			if (progress == null || reporter == null) return source;
			double step = Math.Pow(1 - progressHint, 1 / countHint);
			return Observable.Defer(() =>
			{
				double rest = 1;
				var reporting = reporter.Subscribe(_ => progress.Report(1 - (float)(rest *= step)));
				return source.Finally(reporting.Dispose).ReportOnCompleted(progress);
			});
		}

		public static IObservable<T> FakeProgressOnUpdate<T>(this IObservable<T> source,
			IProgress<float> progress, double frameHint = 100, double progressHint = 0.8) =>
			source.FakeProgress(progress, Observable.EveryUpdate(), frameHint, progressHint);

		public static IObservable<T> FakeProgressOnNext<T>(this IObservable<T> source,
			IProgress<float> progress, double countHint, double progressHint = 0.8)
		{
			if (progress == null) return source;
			double step = Math.Pow(1 - progressHint, 1 / countHint);
			return Observable.Defer(() =>
			{
				double rest = 1;
				return source.Do(_ => progress.Report(1 - (float)(rest *= step))).ReportOnCompleted(progress);
			});
		}
	}
}
