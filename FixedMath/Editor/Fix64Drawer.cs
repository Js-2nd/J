namespace J
{
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(Fix64))]
	public class Fix64Drawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label = EditorGUI.BeginProperty(position, label, property);
			property = property.FindPropertyRelative("m_rawValue");
			EditorGUI.BeginChangeCheck();
			var floatValue = EditorGUI.FloatField(position, label, (float)Fix64.FromRaw(property.longValue));
			if (EditorGUI.EndChangeCheck())
				property.longValue = ((Fix64)floatValue).RawValue;
			EditorGUI.EndProperty();
		}
	}
}
