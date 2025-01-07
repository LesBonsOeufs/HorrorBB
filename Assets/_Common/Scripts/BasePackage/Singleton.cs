using NaughtyAttributes;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
	[Header("Singleton")]
	[SerializeField] private bool dontDestroyOnLoad = false;

	/// <summary>
	/// Gets the instance of the Singleton.
	/// </summary>
	/// <value>The instance.</value>
	public static T Instance
	{
		get
		{
			if (_instance == null)
				_instance = FindObjectOfType<T>(true);

			return _instance;
		}
	}
    private static T _instance;

    protected virtual void Awake()
	{
        if (_instance == null)
            _instance = this as T;
        else if (_instance != this)
		{
            Destroy(gameObject);
			return;
		}

		if (dontDestroyOnLoad)
		{
			transform.parent = null;
			DontDestroyOnLoad(gameObject);
		}
    }

	protected virtual void OnDestroy()
	{
		if (_instance == this)
			_instance = null;
	}
}