using System.Collections;
using UnityEngine;

public class TempleManager : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event playIntroAmbience, stopIntroAmbience;

    public void StartAmbiences()
    {
        playIntroAmbience.Post(gameObject);
    }

    public void StopAmbiences()
    {
        stopIntroAmbience.Post(gameObject);
    }
}
