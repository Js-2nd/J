namespace J
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	[Serializable]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField] List<TKey> m_Keys = new List<TKey>();
		[SerializeField] List<TValue> m_Values = new List<TValue>();

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			m_Keys.Clear();
			m_Values.Clear();
			foreach (var pair in this)
			{
				m_Keys.Add(pair.Key);
				m_Values.Add(pair.Value);
			}
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			Clear();
			int cnt = Mathf.Min(m_Keys.Count, m_Values.Count);
			for (int i = 0; i < cnt; i++)
				Add(m_Keys[i], m_Values[i]);
		}
	}
}
