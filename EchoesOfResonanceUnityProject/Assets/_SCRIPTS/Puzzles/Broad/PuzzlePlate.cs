using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PuzzlePlate : MonoBehaviour, IInputScript
{
    [SerializeField] private Vector3 alignRotation;
    public PuzzleData linkedData;
    public Gem[] gems;
    public BasicPuzzle[] linkedPuzzles;
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

        linkedData.solved = 0;
        active = false;
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

                plateMat.SetColor("_EmissionColor", DH.Get<GlobalPlateData>().activeColor * 5);

                if (executeWithPuzzle != null)
                {
                    executeWithPuzzle.Invoke();
                }

                targetTr = col.GetComponent<Transform>();
                var targetTrData = new TrData(transform.position + Vector3.up * 3.1f, Quaternion.Euler(alignRotation), transform.localScale);
                CRManager.root.Begin(targetTrData.ApplyToOverTime(targetTr, DH.Get<GlobalPlateData>().alignSpeed, true), $"AlignPlayerTo{gameObject.name}", this);
                active = true;
            }
            else if (col.CompareTag("Boulder"))
            {
                targetTr = col.GetComponent<Transform>();
                Debug.Log(transform.position + Vector3.up * (targetTr.localScale.y / 2f));
                var targetTrData = new TrData(transform.position + Vector3.up * (targetTr.localScale.y / 2f), Quaternion.identity, Vector3.zero);
                CRManager.root.Begin(targetTrData.ApplyToOverTime(targetTr, DH.Get<GlobalPlateData>().alignSpeed * 0.5f, true), $"AlignBoulderTo{gameObject.name}", this);
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
            }
            else
            {
                linkedData.solved = 0;
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
        targetTr = null;
        active = false;

        foreach (var p in linkedPuzzles)
        {
            p.FinishedPuzzle();
        }

        plateMat.SetColor("_EmissionColor", DH.Get<GlobalPlateData>().completedColor * 5);
    }
    void OnDisable()
    {
        Destroy(plateMat);
        linkedData.OnSolvedChanged -= solved => UpdateGem(solved);
        linkedData.OnPuzzleCompleted -= () => PlateCompleted();
    }
}