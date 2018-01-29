namespace J
{
	using UnityEngine;

	public class EnumFlagsAttribute : PropertyAttribute
	{
		public int[] Layout;
		public bool ShowIntField;

		public EnumFlagsAttribute(params int[] layout)
		{
			Layout = layout;
		}
	}
}
