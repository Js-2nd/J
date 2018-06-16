namespace J
{
	using System;
	using System.Collections.Generic;

	public class CustomEqualityComparer<T> : IEqualityComparer<T>
	{
		readonly Func<T, T, bool> equals;
		readonly Func<T, int> getHashCode;

		public CustomEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
		{
			this.equals = equals;
			this.getHashCode = getHashCode;
		}

		public bool Equals(T x, T y) => equals(x, y);

		public int GetHashCode(T obj) => getHashCode(obj);
	}
}
