using System.Collections;
using UnityEngine;

public class TempleManager : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event introAmbience;

    public void StartAmbiences()
    {
        introAmbience.Post(gameObject);
    }
}
