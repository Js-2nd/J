using UnityEngine;

namespace J
{
	public class EnumFlagsAttribute : PropertyAttribute
	{
		public int[] layout;

		public EnumFlagsAttribute(params int[] layout)
		{
			this.layout = layout;
		}
	}
}
