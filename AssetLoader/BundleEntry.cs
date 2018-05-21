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
				BundleName = bundleName != null ? bundleName.ToLower() : string.Empty;
			}

			public bool Equals(BundleEntry other) => BundleName == other?.BundleName;

			public override bool Equals(object obj) => Equals(obj as BundleEntry);

			public override int GetHashCode() => BundleName?.GetHashCode() ?? 0;

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
