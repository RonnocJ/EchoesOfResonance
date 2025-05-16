using System.Collections.Generic;
using UnityEngine;

public class PlayerSensor : MonoBehaviour, IInputScript
{
    [SerializeField] private float _humRadius;
    public HashSet<Gem> HummingGems = new();
    private bool _checkingGems;
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, AddGems);
        InputManager.root.AddListener<float>(ActionTypes.KeyUp, RemoveGems);
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Plate") || col.CompareTag("Cutscene"))
        {
            PlrMngr.root.savedPosition = new TrData(PlrMngr.root.transform);
        }
    }
    void AddGems(float newNote)
    {
        var colliders = Physics.OverlapSphere(transform.position, _humRadius);
        var gemsToAdd = new List<Gem>();

        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out Gem g) && g.gemNote.Pitch == newNote)
            {
                bool tooClose = false;

                foreach (var existing in gemsToAdd)
                {
                    if (Vector3.Distance(g.transform.position, existing.transform.position) < 10)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    gemsToAdd.Add(g);
                    HummingGems.Add(g);

                    _checkingGems = true;

                    AudioManager.root.PlaySound(AudioEvent.playGemHum, g.gameObject, 1);
                    AudioManager.root.SetRTPC(AudioRTPC.gemHum_Pitch, newNote, false, AudioEvent.playGemHum, g.gameObject, 1);
                }
            }
        }
    }

    void RemoveGems(float oldNote)
    {
        var gemsToRemove = new List<Gem>();

        foreach (var g in HummingGems)
        {
            if (g.gemNote.Pitch == oldNote)
            {
                AudioManager.root.PlaySound(AudioEvent.stopGemHum, g.gameObject, 1);
                gemsToRemove.Add(g);
            }
        }

        foreach (var g in gemsToRemove)
        {
            HummingGems.Remove(g);
        }

        if (HummingGems.Count == 0) _checkingGems = false;
    }

    void Update()
    {
        if (!_checkingGems) return;

        var colliders = Physics.OverlapSphere(transform.position, _humRadius);

        var gemsToKeep = new HashSet<Gem>();

        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out Gem g) && HummingGems.Contains(g))
            {
                gemsToKeep.Add(g);
            }
        }

        foreach (var g in HummingGems)
        {
            if (!gemsToKeep.Contains(g))
            {
                AudioManager.root.PlaySound(AudioEvent.stopGemHum, g.gameObject, 1);
            }
        }

        HummingGems = gemsToKeep;
    }
}