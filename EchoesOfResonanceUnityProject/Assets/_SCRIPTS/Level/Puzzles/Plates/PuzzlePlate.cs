using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzlePlate : MonoBehaviour, IInputScript, ISaveData
{
    [SerializeField] private Vector3 alignRotation;
    public PuzzleData linkedData;
    public MusicTracker playMusicOnActivate;
    public AudioEffects effectsOnActivate;
    public Gem[] gems;
    public Transform targetTr;
    private bool active;

    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, CheckNote);
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, StartPuzzle);
    }
    public Dictionary<string, object> AddSaveData()
    {
        return new()
        {
            {"solved", linkedData.Solved}
        };
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        linkedData.Active = false;
        linkedData.solved = 0;
        
        if (savedData.TryGetValue("solved", out object isSolvedRaw))
        {
            bool isSolved = Convert.ToBoolean(isSolvedRaw);

            if (isSolved)
            {
                linkedData.solved = linkedData.solutions.Length;

                UpdateGem(linkedData.solutions.Length);
            }

        }
    }
    public void SetupPuzzle()
    {
        linkedData.Active = true;

        linkedData.OnSolvedChanged += UpdateGem;

        linkedData.OnReset += EjectPlayer;
        linkedData.SetMusicComplete();
    }
    public void DeactivatePuzzle()
    {
        linkedData.Active = false;

        linkedData.OnSolvedChanged -= UpdateGem;

        linkedData.OnReset -= EjectPlayer;
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && GameManager.root.State == GameState.Roaming && !linkedData.Solved)
        {
            targetTr = col.GetComponent<Transform>();
            GameManager.root.State = GameState.Synced;

            BrDisplay.root.SetSyncText(linkedData.solutions[0].noteName);
        }
        else if (col.CompareTag("Boulder"))
        {
            targetTr = col.GetComponent<Transform>();
            targetTr.SetParent(transform, true);

            var targetTrData = new TrData(Vector3.up * (targetTr.localScale.y / 2f));
            CRManager.root.Begin(targetTrData.ApplyToOverTime(targetTr, DH.Get<GlobalPlateData>().alignSpeed * 0.5f), $"AlignBoulderTo{gameObject.name}", this);
        }
    }
    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player") && GameManager.root.State == GameState.Synced)
        {
            targetTr = null;
            GameManager.root.State = GameState.Roaming;

            BrDisplay.root.LowerTextPriority(DisplayPriority.Sync);
        }
    }
    [AllowedStates(GameState.Synced)]
    public void StartPuzzle(float noteInput)
    {
        if (noteInput == PzUtil.GetNoteNumber(linkedData.solutions[0].noteName) && targetTr != null)
        {
            linkedData.reset = 0;
            SetupPuzzle();

            BrPitchFinder.root.SetGemList(gems.ToList().Skip(0).Take(FindNextCheckpointIndex(0)).ToList());

            targetTr.SetParent(transform, true);

            var targetTrData = new TrData(Vector3.up * 4.125f, Quaternion.Euler(alignRotation));
            CRManager.root.Begin(targetTrData.ApplyToOverTime(targetTr, DH.Get<GlobalPlateData>().alignSpeed), $"AlignPlayerTo{gameObject.name}", this);
            active = true;

            GameManager.root.State = GameState.InPuzzle;
            GameManager.root.currentPuzzle = linkedData;
            GameManager.root.currentPlate = this;

            if(playMusicOnActivate != null) MusicManager.root.PlaySong(playMusicOnActivate);
            if (effectsOnActivate != null) effectsOnActivate.ExecuteActions();

            CheckNote(noteInput);

            BrDisplay.root.LowerTextPriority(DisplayPriority.Sync);
        }
    }
    [AllowedStates(GameState.InPuzzle)]
    public void CheckNote(float noteInput)
    {
        if (targetTr != null && active)
        {
            if (noteInput == 13)
            {
                linkedData.reset++;
            }
            else
            {
                linkedData.reset = 0;
            }

            if (linkedData.solved < linkedData.solutions.Length && linkedData.reset < 3)
            {
                if (PzUtil.GetNoteNumber(linkedData.solutions[linkedData.solved].noteName) == noteInput)
                {
                    linkedData.solved++;

                    if ((linkedData.solved < linkedData.solutions.Length && linkedData.solutions[linkedData.solved].checkpoint) || linkedData.solved == linkedData.solutions.Length)
                    {
                        for (int i = FindPreviousCheckpointIndex(linkedData.solved - 1); i < linkedData.solved; i++)
                        {
                            MusicManager.root.currentSong.AddQueuedCallback($"{gameObject.name}SequenceSolved", linkedData.solutions[i].noteDuration.CurrentValue, gems[i].CheckpointReached);
                        }

                        if (linkedData.solved < linkedData.solutions.Length)
                        {
                            int startIdx = linkedData.solved;
                            int endIdx = FindNextCheckpointIndex(linkedData.solved);

                            BrPitchFinder.root.SetGemList(gems[startIdx..endIdx].ToList());

                        }
                    }
                }
                else
                {
                    int previousCheckpointIndex = FindPreviousCheckpointIndex(linkedData.solved);
                    linkedData.solved = previousCheckpointIndex;
                }
            }
        }
    }
    private int FindNextCheckpointIndex(int startIndex)
    {
        for (int i = startIndex + 1; i < linkedData.solutions.Length; i++)
        {
            if (linkedData.solutions[i].checkpoint)
            {
                return i;
            }
        }
        return linkedData.solutions.Length;
    }

    private int FindPreviousCheckpointIndex(int currentIndex)
    {
        for (int i = currentIndex; i >= 0; i--)
        {
            if (linkedData.solutions[i].checkpoint)
            {
                return i;
            }
        }
        return 0;
    }
    public void UpdateGem(int solved)
    {
        for (int i = 0; i < gems.Length; i++)
        {
            bool lit = i < solved;
            if (lit && !gems[i].gemLit) gems[i].LightOn();
            else if (!lit && gems[i].gemLit) gems[i].LightOff();
        }
    }
    void EjectPlayer()
    {
        targetTr.SetParent(null, true);

        linkedData.reset = 0;
        linkedData.Active = false;

        active = false;

        if (linkedData.Solved)
        {
            GameManager.root.State = GameState.Roaming;
            targetTr = null;
        }
        else
        {
            linkedData.solved = 0;
            GameManager.root.State = GameState.Synced;
        }
    }

    void OnDisable()
    {
        DeactivatePuzzle();
    }
}