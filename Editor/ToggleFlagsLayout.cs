namespace J
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public class ToggleFlagsLayout : IReadOnlyList<ToggleFlagsLayout.Line>
	{
		public float TotalLineWeight { get; private set; }

		readonly List<Line> lines = new List<Line>();

		public ToggleFlagsLayout NewLine(float? weight = null)
		{
			var line = new Line(weight);
			TotalLineWeight += line.LineWeight;
			lines.Add(line);
			return this;
		}

		public ToggleFlagsLayout Add(Enum value, string name = null, float? weight = null)
		{
			if (Count == 0) NewLine();
			this[Count - 1].Add(value, name, weight);
			return this;
		}

		public Line this[int index] => lines[index];
		public int Count => lines.Count;
		public IEnumerator<Line> GetEnumerator() => lines.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public class Line : IReadOnlyList<LineItem>
		{
			public float LineWeight { get; }
			public float TotalItemWeight { get; private set; }

			readonly List<LineItem> items;

			public Line(float? weight = null)
			{
				LineWeight = weight ?? 1;
				items = new List<LineItem>();
			}

			public void Add(Enum value, string name = null, float? weight = null)
			{
				var item = new LineItem(value, name, weight);
				TotalItemWeight += item.Weight;
				items.Add(item);
			}

			public LineItem this[int index] => items[index];
			public int Count => items.Count;
			public IEnumerator<LineItem> GetEnumerator() => items.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		public class LineItem
		{
			public long? Value { get; }
			public string Name { get; }
			public float Weight { get; }

			public LineItem(Enum value, string name = null, float? weight = null)
			{
				if (value != null)
				{
					Value = Convert.ToInt64(value);
					Name = name ?? value.ToString();
				}
				Weight = weight ?? 1;
			}
		}
	}
}
