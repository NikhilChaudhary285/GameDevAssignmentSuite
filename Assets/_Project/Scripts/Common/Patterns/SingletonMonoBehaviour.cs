using UnityEngine;

namespace TechnicalAssignment.Common.Patterns
{
    /// <summary>
    /// Thread-safe, DontDestroyOnLoad Singleton base for MonoBehaviours.
    /// WHY: We need exactly one instance of managers (ImageDownloader, etc.)
    /// that persist across scene loads without duplication.
    /// 
    /// DESIGN DECISION: Abstract class, not interface.
    /// Unity requires MonoBehaviour inheritance — we can't inject via interface here.
    /// But we DO expose interfaces for the actual logic (see ImageDownloader).
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
        where T : SingletonMonoBehaviour<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} requested after application quit. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            var singletonObject = new GameObject($"[Singleton] {typeof(T).Name}");
                            _instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);

                            Debug.Log($"[Singleton] Created instance of {typeof(T).Name}");
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)this;
                DontDestroyOnLoad(gameObject);
                OnAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T).Name} destroyed.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnAwake() { }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }
    }
}