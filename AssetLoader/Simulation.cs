using System.Linq;

namespace J
{
	public partial class AssetLoaderInstance
	{

		string GetAssetPath(AssetEntry entry)
		{
			string path = null;
			path = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(entry.BundleName, entry.AssetName)?.FirstOrDefault();
			return path;
		}

		public enum Simulation
		{
			Disable = 0,
			AssetDatabase = 1,
			AssetGraph = 2,
		}
	}
}
