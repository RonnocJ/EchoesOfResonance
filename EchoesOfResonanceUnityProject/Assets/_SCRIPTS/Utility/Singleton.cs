using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _root;
    private static bool applicationIsQuitting = false;
    public static T root
    {
        get
        {
            if (_root == null)
            {
                if (applicationIsQuitting) return null;

                _root = FindFirstObjectByType<T>();
                if (_root == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _root = singletonObject.AddComponent<T>();
                    Debug.LogError($"No Singleton instance of {_root} found, creating new game object");
                }
            }
            return _root;
        }
    }

    protected virtual void Awake()
    {
        if (_root == null)
        {
            _root = this as T;
        }
        else if (_root != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }
}
