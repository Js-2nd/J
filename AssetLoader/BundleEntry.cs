namespace J
{
	using System;

	partial class AssetLoaderInstance
	{
		public sealed class BundleEntry : IEquatable<BundleEntry>
		{
			public string BundleName { get; }

			public BundleEntry(string bundleName)
			{
				if (bundleName == null)
					throw new ArgumentNullException(nameof(bundleName));
				BundleName = bundleName.ToLower();
			}

			public bool Equals(BundleEntry other) => BundleName == other?.BundleName;

			public override bool Equals(object obj) => Equals(obj as BundleEntry);

			public override int GetHashCode() => BundleName.GetHashCode();

			public static bool operator ==(BundleEntry lhs, BundleEntry rhs)
			{
				if (ReferenceEquals(lhs, null))
					return ReferenceEquals(rhs, null);
				return lhs.Equals(rhs);
			}

			public static bool operator !=(BundleEntry lhs, BundleEntry rhs) => !(lhs == rhs);
		}
	}
}
