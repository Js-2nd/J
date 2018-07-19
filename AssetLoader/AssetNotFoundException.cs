namespace J
{
	using System;

	[Serializable]
	public class AssetNotFoundException : Exception
	{
		public AssetEntry AssetEntry { get; }
		public BundleEntry BundleEntry { get; }
		public override string Message { get; }

		public AssetNotFoundException(AssetEntry entry)
		{
			AssetEntry = entry;
			BundleEntry = entry.BundleEntry;
			Message = $"Asset not found. {entry}";
		}

		public AssetNotFoundException(BundleEntry entry)
		{
			BundleEntry = entry;
			Message = $"AssetBundle not found. {entry}";
		}
	}
}
