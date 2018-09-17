namespace J
{
	using UnityEditor;
	using UnityEngine;

	public abstract class ToggleFlagsDrawer : PropertyDrawer
	{
		public ToggleFlagsLayout Layout = new ToggleFlagsLayout();

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			Layout.TotalLineWeight * EditorGUIUtility.singleLineHeight;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.LabelField(position, label);
			EditorGUI.BeginChangeCheck();
			long value = property.longValue;
			position.xMin += EditorGUIUtility.labelWidth;
			var rect = position;
			for (int i = 0; i < Layout.Count; i++)
			{
				var line = Layout[i];
				rect.height = line.LineWeight * EditorGUIUtility.singleLineHeight;
				for (int j = 0; j < line.Count; j++)
				{
					var item = line[j];
					rect.width = position.width * item.Weight / line.TotalItemWeight;
					if (item.Value.HasValue)
					{
						long flag = item.Value.Value;
						bool toggle = value.HasFlag(flag);
						if (GUI.Toggle(rect, toggle, item.Name, "Button") != toggle)
							value = toggle ? value.UnsetFlag(flag) : value.SetFlag(flag);
					}
					rect.x += rect.width;
				}
				rect.y += rect.height;
				rect.x = position.x;
			}
			if (EditorGUI.EndChangeCheck()) property.longValue = value;
			EditorGUI.EndProperty();
		}
	}
}
