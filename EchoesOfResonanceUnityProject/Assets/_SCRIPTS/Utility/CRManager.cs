using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CRManager : Singleton<CRManager>
{
    private readonly Dictionary<string, Coroutine> routineDict = new();

    public void Begin(IEnumerator routine, string key, MonoBehaviour root)
    {
        if (!routineDict.ContainsKey(key))
        {
            Coroutine newCoroutine = root.StartCoroutine(ManagedCoroutine(routine, key));
            routineDict.Add(key, newCoroutine);
        }
    }

    public void Restart(IEnumerator routine, string key, MonoBehaviour root)
    {
        if (routineDict.TryGetValue(key, out Coroutine existingRoutine))
        {
            root.StopCoroutine(existingRoutine);
            routineDict.Remove(key);

            Coroutine newCoroutine = root.StartCoroutine(ManagedCoroutine(routine, key));
            routineDict.Add(key, newCoroutine);
        }
        else
        {
            Coroutine newCoroutine = root.StartCoroutine(ManagedCoroutine(routine, key));
            routineDict.Add(key, newCoroutine);
        }
    }

    public void Stop(string key, MonoBehaviour root)
    {
        if (routineDict.TryGetValue(key, out Coroutine existing))
        {
            root.StopCoroutine(existing);
            routineDict.Remove(key);
        }
    }

    private IEnumerator ManagedCoroutine(IEnumerator routine, string key)
    {
        yield return routine;

        routineDict.Remove(key);
    }
    public bool IsRunning(string key)
    {
        return routineDict.ContainsKey(key);
    }
}