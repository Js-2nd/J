namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
	public class EnumFlagsDrawer : PropertyDrawer
	{
		const float ZeroHeight = 1 / 3f;

		List<int> layout;
		bool showIntField;
		KeyValuePair<int, string>[] flags;
		int everythingValue;
		float propertyHeight;

		public EnumFlagsDrawer() { }
		public EnumFlagsDrawer(params int[] layout)
		{
			this.layout = layout.ToList();
		}

		void Init()
		{
			if (flags != null) return;

			EnumFlagsAttribute attr = attribute as EnumFlagsAttribute;
			if (attr != null)
			{
				layout = attr.Layout.ToList();
				showIntField = attr.ShowIntField;
			}
			if (layout == null) layout = new List<int>();

			var values = Enum.GetValues(fieldInfo.FieldType);
			var names = Enum.GetNames(fieldInfo.FieldType);
			var single = new SortedDictionary<int, string>();
			var multi = new SortedDictionary<int, string>();
			for (int i = 0; i < values.Length; i++)
			{
				int value = (int)values.GetValue(i);
				if (value == 0) continue;

				string name = names[i];
				if (value == (value & -value)) single.Add(value, name);
				else multi.Add(value, name);
				everythingValue = everythingValue.SetFlag(value);
			}
			flags = single.Concat(multi).ToArray();

			int diff = flags.Length - layout.Sum();
			int sum = 0;
			if (diff > 0) layout.AddRange(new int[diff].Select(i => 1));
			else layout = layout.TakeWhile(i => (sum += i) - i < flags.Length).ToList();
			int zeroCount = layout.Where(value => value == 0).Count();
			propertyHeight = EditorGUIUtility.singleLineHeight * (1 + layout.Count - zeroCount + zeroCount * ZeroHeight);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Init();
			return propertyHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Init();
			EditorGUI.LabelField(position, label);
			EditorGUI.BeginChangeCheck();
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = position.width - labelWidth;
			float fieldX = position.x + labelWidth;
			bool state;
			Rect pos = position;
			pos.x = fieldX;
			pos.width = fieldWidth / (showIntField ? 3 : 2);
			pos.height = EditorGUIUtility.singleLineHeight;
			int value = property.intValue;
			if (showIntField)
			{
				value = EditorGUI.IntField(pos, value);
				pos.x += pos.width;
			}
			if (GUI.Toggle(pos, state = value == 0, "Nothing", "Button") != state) value = 0;
			pos.x += pos.width;
			if (GUI.Toggle(pos, state = value == everythingValue, "Everything", "Button") != state) value = everythingValue;
			pos.y += pos.height;

			for (int y = 0, i = 0; i < flags.Length; y++)
			{
				int column = layout[y];
				if (column <= 0)
				{
					pos.y += pos.height * ZeroHeight;
					continue;
				}

				pos.x = fieldX;
				pos.width = fieldWidth / column;
				for (int x = 0; x < column && i < flags.Length; i++, x++)
				{
					var item = flags[i];
					int flag = item.Key;
					if (GUI.Toggle(pos, state = value.HasFlag(flag), item.Value, "Button") != state)
						value = state ? value.UnsetFlag(flag) : value.SetFlag(flag);
					pos.x += pos.width;
				}
				pos.y += pos.height;
			}

			if (EditorGUI.EndChangeCheck()) property.intValue = value;
		}
	}
}
