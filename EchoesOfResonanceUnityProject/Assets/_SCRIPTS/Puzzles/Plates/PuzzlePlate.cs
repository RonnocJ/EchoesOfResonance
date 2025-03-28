using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PuzzlePlate : MonoBehaviour, IInputScript
{
    [SerializeField] private Vector3 alignRotation;
    public PuzzleData linkedData;
    public Gem[] gems;
    [SerializeField] private UnityEvent executeWithPuzzle;
    private bool active;
    private Transform targetTr;
    private Material plateMat;
    void Awake()
    {
        plateMat = GetComponent<MeshRenderer>().materials[1];

        linkedData.OnSolvedChanged += solved => UpdateGem(solved);
        linkedData.OnPuzzleCompleted += () => PlateCompleted();
        linkedData.OnReset += () => EjectPlayer();
        linkedData.SetMusicComplete();

        linkedData.solved = 0;
        linkedData.reset = 0;
        active = false;

    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, CheckNote);
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && GameManager.root.currentState == GameState.Roaming && linkedData.solved < linkedData.solutions.Length)
        {
            GameManager.root.currentState = GameState.InPuzzle;
            GameManager.root.currentPuzzle = linkedData;
            GameManager.root.currentPlate = this;

            int firstCheckpointIndex = FindNextCheckpointIndex(0);

            BrPitchFinder.root.SetGemList(gems.ToList().Skip(0).Take(firstCheckpointIndex).ToList());

            plateMat.SetColor("_EmissionColor", DH.Get<GlobalPlateData>().activeColor * 5);

            if (executeWithPuzzle != null)
            {
                executeWithPuzzle.Invoke();
            }

            foreach (var gem in gems)
            {
                CRManager.root.Begin(gem.ShiftGem(gem.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>(), 2), $"ShiftGem{gem.name}", this);
            }

            targetTr = col.GetComponent<Transform>();

            targetTr.SetParent(transform, true);

            var targetTrData = new TrData(Vector3.up * 4.125f, Quaternion.Euler(alignRotation));
            CRManager.root.Begin(targetTrData.ApplyToOverTime(targetTr, DH.Get<GlobalPlateData>().alignSpeed), $"AlignPlayerTo{gameObject.name}", this);
            active = true;
        }
        else if (col.CompareTag("Boulder"))
        {
            targetTr = col.GetComponent<Transform>();
            targetTr.SetParent(transform, true);

            var targetTrData = new TrData(Vector3.up * (targetTr.localScale.y / 2f));
            CRManager.root.Begin(targetTrData.ApplyToOverTime(targetTr, DH.Get<GlobalPlateData>().alignSpeed * 0.5f), $"AlignBoulderTo{gameObject.name}", this);
        }
    }
    [AllowedStates(GameState.InPuzzle)]
    public void CheckNote(float noteInput)
    {
        if (targetTr != null && active)
        {
            float note = PuzzleUtilities.root.GetNoteNumber(linkedData.solutions[linkedData.solved].noteName);

            if (noteInput == 13)
            {
                linkedData.reset++;
            }
            else
            {
                linkedData.reset = 0;
            }

            if (note == noteInput && linkedData.reset < 3)
            {
                linkedData.solved++;

                if (linkedData.solved < linkedData.solutions.Length && linkedData.solutions[linkedData.solved].checkpoint)
                {
                    for (int i = FindPreviousCheckpointIndex(linkedData.solved - 1); i < linkedData.solved; i++)
                    {
                        MusicManager.root.currentSong.AddQueuedCallback($"{linkedData.name}SequenceSolved", linkedData.solutions[i].noteDuration.CurrentValue, gems[i].CheckpointReached);
                    }

                    int startIdx = linkedData.solved;
                    int endIdx = FindNextCheckpointIndex(linkedData.solved);

                    BrPitchFinder.root.SetGemList(gems.ToList().Skip(startIdx).Take(endIdx - startIdx).ToList());
                }
            }
            else
            {
                int previousCheckpointIndex = FindPreviousCheckpointIndex(linkedData.solved);
                linkedData.solved = previousCheckpointIndex;
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
        for (int i = 0; i < solved; i++)
        {
            if (!gems[i].gemLit)
            {
                gems[i].LightOn();
                CRManager.root.Begin(gems[i].ShiftGem(gems[i].transform.GetChild(0).GetComponent<SkinnedMeshRenderer>(), 4), $"ShiftGem{gems[i].name}", this);
            }
                
        }
        for (int j = linkedData.solutions.Length - 1; j >= solved; j--)
        {
            if (gems[j].gemLit)
                gems[j].LightOff();
        }
    }
    void EjectPlayer()
    {
        linkedData.solved = 0;
        active = false;
        targetTr.SetParent(null, true);

        if (targetTr.TryGetComponent(out Rigidbody rb))
        {
            Vector2 sideForce = Random.insideUnitCircle * DH.Get<GlobalPlateData>().ejectForce;
            rb.AddForce(new Vector3(sideForce.x, DH.Get<GlobalPlateData>().ejectForce, sideForce.y));
        }

        CRManager.root.Begin(Cooldown(), $"{gameObject}Cooldown", this);

        GameManager.root.currentState = GameState.Roaming;
    }
    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(2f);
        targetTr = null;
        linkedData.reset = 0;
    }
    void PlateCompleted()
    {
        GameManager.root.currentState = GameState.Roaming;
        targetTr.parent = null;
        targetTr = null;
        active = false;

        for (int i = FindPreviousCheckpointIndex(linkedData.solved - 1); i < linkedData.solved; i++)
        {
            MusicManager.root.currentSong.AddQueuedCallback($"{linkedData.name}SequenceSolved", linkedData.solutions[i].noteDuration.CurrentValue, gems[i].CheckpointReached);
        }

        plateMat.SetColor("_EmissionColor", DH.Get<GlobalPlateData>().completedColor * 5);
    }
    void OnDisable()
    {
        linkedData.OnSolvedChanged -= solved => UpdateGem(solved);
        linkedData.OnPuzzleCompleted -= () => PlateCompleted();
    }
}