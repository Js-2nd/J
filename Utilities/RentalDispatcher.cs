namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;

	public class RentalDispatcher : IObservable<IDisposable>
	{
		readonly LinkedList<Subscription> pendingList = new LinkedList<Subscription>();
		int count;
		int capacity;

		public RentalDispatcher(int capacity = 16)
		{
			this.capacity = capacity;
		}

		public int Capacity
		{
			get { return capacity; }
			set
			{
				capacity = value;
				Run();
			}
		}

		void Run()
		{
			while (pendingList.Count > 0 && count < capacity)
			{
				var observer = pendingList.First.Value.Observer;
				pendingList.RemoveFirst();
				Rent(observer);
			}
		}

		public IDisposable Subscribe(IObserver<IDisposable> observer)
		{
			if (pendingList.Count == 0 && count < capacity)
			{
				Rent(observer);
				return Disposable.Empty;
			}

			return new Subscription(observer, this);
		}

		void Rent(IObserver<IDisposable> observer)
		{
			observer.OnNext(new Rental(this));
			observer.OnCompleted();
		}

		class Subscription : IDisposable
		{
			internal readonly IObserver<IDisposable> Observer;
			readonly LinkedListNode<Subscription> node;

			public Subscription(IObserver<IDisposable> observer, RentalDispatcher dispatcher)
			{
				Observer = observer;
				node = dispatcher.pendingList.AddLast(this);
				dispatcher.Run();
			}

			public void Dispose() => node.List?.Remove(node);
		}

		class Rental : IDisposable
		{
			RentalDispatcher dispatcher;

			public Rental(RentalDispatcher dispatcher)
			{
				this.dispatcher = dispatcher;
				dispatcher.count++;
			}

			public void Dispose()
			{
				if (dispatcher == null) return;
				var d = dispatcher;
				dispatcher = null;
				d.count--;
				d.Run();
			}
		}
	}
}