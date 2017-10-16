namespace J
{
	using UnityEngine;

	public class EnumFlagsAttribute : PropertyAttribute
	{
		public int[] layout;

		public EnumFlagsAttribute(params int[] layout)
		{
			this.layout = layout;
		}
	}
}
