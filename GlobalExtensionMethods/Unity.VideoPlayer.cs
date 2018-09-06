using System;
using UniRx;
using UnityEngine.Video;

public static partial class GlobalExtensionMethods
{
	public static IObservable<double> ObserveClockResyncOccurred(this VideoPlayer player)
	{
		return Observable.FromEvent<VideoPlayer.TimeEventHandler, double>(
			action => (source, seconds) => action(seconds),
			handler => player.clockResyncOccurred += handler,
			handler => player.clockResyncOccurred -= handler);
	}

	public static IObservable<string> ObserveErrorReceived(this VideoPlayer player)
	{
		return Observable.FromEvent<VideoPlayer.ErrorEventHandler, string>(
			action => (source, message) => action(message),
			handler => player.errorReceived += handler,
			handler => player.errorReceived -= handler);
	}

	public static IObservable<VideoPlayer> ObserveFrameDropped(this VideoPlayer player)
	{
		return Observable.FromEvent<VideoPlayer.EventHandler, VideoPlayer>(
			action => source => action(source),
			handler => player.frameDropped += handler,
			handler => player.frameDropped -= handler);
	}

	public static IObservable<long> ObserveFrameReady(this VideoPlayer player)
	{
		return Observable.FromEvent<VideoPlayer.FrameReadyEventHandler, long>(
			action => (source, frameIdx) => action(frameIdx),
			handler => player.frameReady += handler,
			handler => player.frameReady -= handler);
	}

	public static IObservable<VideoPlayer> ObserveLoopPointReached(this VideoPlayer player)
	{
		return Observable.FromEvent<VideoPlayer.EventHandler, VideoPlayer>(
			action => source => action(source),
			handler => player.loopPointReached += handler,
			handler => player.loopPointReached -= handler);
	}

	public static IObservable<VideoPlayer> ObservePrepareCompleted(this VideoPlayer player)
	{
		return Observable.FromEvent<VideoPlayer.EventHandler, VideoPlayer>(
			action => source => action(source),
			handler => player.prepareCompleted += handler,
			handler => player.prepareCompleted -= handler);
	}

	public static IObservable<VideoPlayer> ObserveSeekCompleted(this VideoPlayer player)
	{
		return Observable.FromEvent<VideoPlayer.EventHandler, VideoPlayer>(
			action => source => action(source),
			handler => player.seekCompleted += handler,
			handler => player.seekCompleted -= handler);
	}

	public static IObservable<VideoPlayer> ObserveStarted(this VideoPlayer player)
	{
		return Observable.FromEvent<VideoPlayer.EventHandler, VideoPlayer>(
			action => source => action(source),
			handler => player.started += handler,
			handler => player.started -= handler);
	}
}
