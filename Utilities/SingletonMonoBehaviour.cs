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
				Instance = (T)this;
				SingletonAwake();
			}
			else
			{
				string message = $"Destroy {typeof(T).Name} on {name} since there is one on {Instance.name}.";
				Debug.LogWarning(message, gameObject);
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
