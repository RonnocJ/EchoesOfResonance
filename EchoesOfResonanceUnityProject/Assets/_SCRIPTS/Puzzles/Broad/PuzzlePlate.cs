using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Vector3 = UnityEngine.Vector3;

public class PuzzlePlate : MonoBehaviour, IInputScript
{
    [SerializeField] private float playerAlignSpeed;
    [SerializeField] private Vector3 playerAlignRotation;
    public PuzzleData linkedData;
    public BasicPuzzle[] linkedPuzzles;
    [SerializeField] private UnityEvent executeWithPuzzle;
    [SerializeField] private Color activePlateColor, completedPlateColor;
    private Gem[] gems;
    private Transform playerTr;
    private Material plateMat;
    void Awake()
    {
        plateMat = GetComponent<MeshRenderer>().material;
        linkedData.OnValueChanged += solved => UpdateGem(solved);
        linkedData.OnPuzzleCompleted += () => PlateCompleted();
        gems = GetComponentsInChildren<Gem>();

        if(gems.Length != linkedData.solutions.Length)
        {
            Debug.LogError($"Update {gameObject} so that the gems and linked puzzle data match! Found {gems.Length} gems and {linkedData.solutions.Length} puzzle solution entries");
        }
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

            plateMat.SetColor("_EmissionColor", activePlateColor * 5);

            if (linkedData.hasOrphans)
            {
                linkedPuzzles[0].orphanParent = GameObject.Find($"{linkedData}GemParent").transform;
                Debug.Log(linkedPuzzles[0].orphanParent);
            }

            if (executeWithPuzzle != null)
            {
                executeWithPuzzle.Invoke();
            }

            playerTr = col.GetComponent<Transform>();
            var targetTr = new TrData(transform.position, transform.rotation, transform.localScale);
            CRManager.root.Begin(targetTr.ApplyToOverTime(playerTr, 2f, true), $"AlignPlayerTo{gameObject.name}", this);
        }
    }
    [AllowedStates(GameState.InPuzzle)]
    public void CheckNote(float noteInput)
    {
        if(playerTr != null)
        {
            if(noteInput == PuzzleManager.root.GetNoteNumber(linkedData.solutions[linkedData.solved].correctNote))
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
        for(int i = 0; i < solved; i++)
        {
            if(!gems[i].gemLit)
                gems[i].LightOn();
        }
        for(int j = linkedData.solutions.Length; j > solved; j--)
        {
            if(gems[j].gemLit)
                gems[j].LightOff();
        }
    }
    void PlateCompleted()
    {
        playerTr = null;
        foreach (var p in linkedPuzzles)
        {
            p.FinishedPuzzle();
        }

        plateMat.SetColor("_EmissionColor", completedPlateColor * 5);
    }
    void OnDestroy()
    {
        linkedData.OnPuzzleCompleted -= () => PlateCompleted();
    }
}