using J;

public static partial class GlobalExtensionMethods
{
	public static float ToFloat(this Fix64 value) => (float)value;
	public static Fix64 ToFix64(this float value) => (Fix64)value;
}
