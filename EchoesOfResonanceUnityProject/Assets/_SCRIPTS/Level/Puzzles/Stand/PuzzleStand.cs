using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class PuzzleStand : MonoBehaviour, ISaveData
{
    [SerializeField] private Vector3 alignRotation;
    protected PuzzleData activeData;
    public MusicTracker playMusicOnActivate;
    public AudioEffects effectsOnActivate;
    public Gem[] gems;
    public TextMeshProUGUI progressText;
    public Transform standTr;
    public Transform targetTr;
    private float _cooldownTime;
    private List<Action> _lightOffMethods = new();
    public Dictionary<string, object> AddSaveData()
    {
        return new()
        {
            {"solved", activeData.Solved}
        };
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        if (savedData.TryGetValue("solved", out object isSolvedRaw))
        {
            bool isSolved = Convert.ToBoolean(isSolvedRaw);

            if (isSolved)
            {
                activeData.solved = activeData.solutions.Length;

                UpdateGem(activeData.solutions.Length);
            }
        }
    }
    public void SetupPuzzle()
    {
        if (activeData.Active) return;

        activeData.Active = true;

        activeData.reset = 0;

        activeData.OnSolvedChanged += UpdateGem;
        activeData.OnReset += EjectPlayer;

        activeData.SetMusicComplete();

        _lightOffMethods.Clear();

        Broadcaster.activePlate = this;
        Broadcaster.activePuzzle = activeData;

        for (int i = 0; i < gems.Length; i++)
        {
            if (gems[i].needsLight)
            {
                _lightOffMethods.Add(() => GemLightOff(i));
                gems[i].OnLightOff += _lightOffMethods[i];
            }
        }
    }
    public void DeactivatePuzzle()
    {
        if (!activeData.Active) return;

        activeData.Active = false;

        activeData.OnSolvedChanged -= UpdateGem;
        activeData.OnReset -= EjectPlayer;

        Broadcaster.activePlate = null;
        Broadcaster.activePuzzle = null;

        for (int i = 0; i < gems.Length; i++)
        {
            if (gems[i].needsLight && i < _lightOffMethods.Count)
            {
                gems[i].OnLightOff -= _lightOffMethods[i];
            }
        }
    }
    void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Player") && GameManager.root.State == GameState.Roaming && !activeData.Solved && Time.timeSinceLevelLoad - _cooldownTime > 2f && targetTr == null)
        {
            targetTr = col.GetComponent<Transform>();
            SetupPuzzle();

            GameManager.root.State = GameState.Synced;
        }
        else if (col.CompareTag("Boulder") && targetTr == null)
        {
            targetTr = col.GetComponent<Transform>();
            targetTr.SetParent(transform, true);

            var targetTrData = new TrData(Vector3.up * (targetTr.localScale.y / 2f));
            CRManager.Begin(targetTrData.ApplyToOverTime(targetTr, 0.25f), $"AlignBoulderTo{gameObject.name}", this);
        }
    }
    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player") && GameManager.root.State == GameState.Synced)
        {
            DeactivatePuzzle();

            targetTr = null;
            GameManager.root.State = GameState.Roaming;
        }
    }
    public void StartPuzzle()
    {
        targetTr.SetParent(transform, true);
        targetTr.GetComponent<Collider>().enabled = false;
        targetTr.GetComponent<Rigidbody>().isKinematic = true;

        var targetTrData = new TrData(Vector3.up * 4.125f, Quaternion.Euler(alignRotation));
        CRManager.Begin(targetTrData.ApplyToOverTime(targetTr, 0.25f), $"AlignPlayerTo{gameObject.name}", this);

        GameManager.root.State = GameState.InPuzzle;

        if (playMusicOnActivate != null) MusicManager.root.PlaySong(playMusicOnActivate);
        if (effectsOnActivate != null) effectsOnActivate.ExecuteActions();

        playMusicOnActivate = null;
        effectsOnActivate = null;
    }
    public void UpdateGem(int solved)
    {
        for (int i = 0; i < gems.Length; i++)
        {
            bool lit = solved > i;
            if (lit && !gems[i].gemLit) gems[i].LightOn();
            else if (!lit && gems[i].gemLit) gems[i].LightOff();
        }
    }
    void GemLightOff(int index)
    {
        index--;

        if (index <= activeData.solved)
        {
            activeData.solved = activeData.FindLastCheckpoint(index - 1);
        }
    }
    public virtual void EjectPlayer()
    {
        if (GameManager.root.State == GameState.InPuzzle) GameManager.root.State = GameState.Roaming;

        targetTr.SetParent(null, true);
        targetTr.GetComponent<Collider>().enabled = true;
        targetTr.GetComponent<Rigidbody>().isKinematic = false;
        targetTr = null;

        if (!activeData.Solved)
        {
            activeData.solved = 0;
        }

        activeData.reset = 0;
        activeData.Active = false;

        _cooldownTime = Time.timeSinceLevelLoad;

        DeactivatePuzzle();
    }

    void OnDisable()
    {
        DeactivatePuzzle();
    }
}