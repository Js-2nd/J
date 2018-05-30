namespace J
{
	using System;

	partial class AssetLoaderInstance
	{
		public static string NormBundleName(string bundleName) => bundleName.ToLower();
	}

	public sealed class BundleEntry : IEquatable<BundleEntry>
	{
		public string NormBundleName { get; }

		public BundleEntry(string bundleName)
		{
			if (bundleName == null) throw new ArgumentNullException(nameof(bundleName));
			NormBundleName = AssetLoaderInstance.NormBundleName(bundleName);
		}

		public bool Equals(BundleEntry other) => NormBundleName == other?.NormBundleName;

		public override bool Equals(object obj) => Equals(obj as BundleEntry);

		public override int GetHashCode() => NormBundleName.GetHashCode();

		public static bool operator ==(BundleEntry lhs, BundleEntry rhs)
		{
			if (ReferenceEquals(lhs, null)) return ReferenceEquals(rhs, null);
			return lhs.Equals(rhs);
		}

		public static bool operator !=(BundleEntry lhs, BundleEntry rhs) => !(lhs == rhs);
	}
}
