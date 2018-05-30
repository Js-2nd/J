namespace J
{
	using System;

	partial class AssetLoaderInstance
	{
		public static string NormBundleName(string bundleName) => bundleName.ToLower();
	}

	public sealed class BundleEntry : IEquatable<BundleEntry>
	{
		public string NormedBundleName { get; }

		public BundleEntry(string bundleName)
		{
			if (bundleName == null) throw new ArgumentNullException(nameof(bundleName));
			NormedBundleName = AssetLoaderInstance.NormBundleName(bundleName);
		}

		public bool Equals(BundleEntry other) => NormedBundleName == other?.NormedBundleName;

		public override bool Equals(object obj) => Equals(obj as BundleEntry);

		public override int GetHashCode() => NormedBundleName.GetHashCode();

		public static bool operator ==(BundleEntry lhs, BundleEntry rhs)
		{
			if (ReferenceEquals(lhs, null)) return ReferenceEquals(rhs, null);
			return lhs.Equals(rhs);
		}

		public static bool operator !=(BundleEntry lhs, BundleEntry rhs) => !(lhs == rhs);
	}
}
