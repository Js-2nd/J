namespace J
{
	using UnityEngine;

	public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
	{
		public static T Instance { get; private set; }

		protected virtual void Awake()
		{
			if (Instance == null || Instance == this)
			{
				Instance = this as T;
				SingletonAwake();
			}
			else
			{
				Debug.LogWarningFormat("Destroy {0} on {1} since there is one on {2}.", typeof(T).Name, name, Instance.name);
				Destroy(this);
			}
		}

		protected virtual void OnDestroy()
		{
			if (Instance != null && Instance == this)
			{
				SingletonOnDestroy();
			}
		}

		protected virtual void SingletonAwake() { }
		protected virtual void SingletonOnDestroy() { }
	}
}
