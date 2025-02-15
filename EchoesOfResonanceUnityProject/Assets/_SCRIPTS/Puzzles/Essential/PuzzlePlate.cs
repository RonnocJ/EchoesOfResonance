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
    public int chpIndex;
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

        chpIndex = -1;
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, CheckNote);
    }
    void OnTriggerEnter(Collider col)
    {
        if (targetTr == null && linkedData.solved < linkedData.solutions.Length)
        {
            if (col.CompareTag("Player") && GameManager.root.currentState == GameState.Roaming)
            {
                GameManager.root.currentState = GameState.InPuzzle;
                GameManager.root.currentPuzzle = linkedData;
                GameManager.root.currentPlate = this;

                BrPitchFinder.root.SetGemList(gems.ToList().Skip(0).Take((linkedData.checkpoints.Length > 0)? linkedData.checkpoints[0] : linkedData.solutions.Length).ToList());

                plateMat.SetColor("_EmissionColor", DH.Get<GlobalPlateData>().activeColor * 5);

                if (executeWithPuzzle != null)
                {
                    executeWithPuzzle.Invoke();
                }

                targetTr = col.GetComponent<Transform>();
                targetTr.SetParent(transform, true);

                var targetTrData = new TrData(Vector3.up * 4.125f, Quaternion.Euler(alignRotation), transform.localScale);
                CRManager.root.Begin(targetTrData.ApplyToOverTime(targetTr, DH.Get<GlobalPlateData>().alignSpeed), $"AlignPlayerTo{gameObject.name}", this);
                active = true;
            }
            else if (col.CompareTag("Boulder"))
            {
                targetTr = col.GetComponent<Transform>();
                targetTr.SetParent(transform, true);

                var targetTrData = new TrData(Vector3.up * (targetTr.localScale.y / 2f), Quaternion.identity, Vector3.zero);
                CRManager.root.Begin(targetTrData.ApplyToOverTime(targetTr, DH.Get<GlobalPlateData>().alignSpeed * 0.5f), $"AlignBoulderTo{gameObject.name}", this);
            }
        }
    }
    [AllowedStates(GameState.InPuzzle)]
    public void CheckNote(float noteInput)
    {
        if (targetTr != null && active)
        {
            float note = PuzzleUtilities.root.GetNoteNumber(linkedData.solutions[linkedData.solved]);

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

                if (linkedData.checkpoints.Length > 0 && chpIndex + 1 < linkedData.checkpoints.Length && linkedData.solved == linkedData.checkpoints[chpIndex + 1])
                {
                    chpIndex++;

                    

                    int startIdx = linkedData.checkpoints[chpIndex];
                    int endIdx = (chpIndex + 1 < linkedData.checkpoints.Length) ? linkedData.checkpoints[chpIndex + 1] : linkedData.solutions.Length;

                    AudioManager.root.PlaySound(AudioEvent.playCheckpointReached, gems[startIdx - 1].gameObject);
                    BrPitchFinder.root.SetGemList(gems.ToList().Skip(startIdx).Take(endIdx - startIdx).ToList());
                }
            }
            else
            {
                if (linkedData.checkpoints.Length > 0)
                {
                    linkedData.solved = (chpIndex >= 0) ? linkedData.checkpoints[chpIndex] : 0;
                }
                else
                {
                    linkedData.solved = 0;
                }
            }

        }
    }
    public void UpdateGem(int solved)
    {
        for (int i = 0; i < solved; i++)
        {
            if (!gems[i].gemLit)
                gems[i].LightOn();
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
        targetTr.parent = null;
        targetTr = null;
        active = false;
        GameManager.root.currentState = GameState.Roaming;

        plateMat.SetColor("_EmissionColor", DH.Get<GlobalPlateData>().completedColor * 5);
    }
    void OnDisable()
    {
        Destroy(plateMat);
        linkedData.OnSolvedChanged -= solved => UpdateGem(solved);
        linkedData.OnPuzzleCompleted -= () => PlateCompleted();
    }
}