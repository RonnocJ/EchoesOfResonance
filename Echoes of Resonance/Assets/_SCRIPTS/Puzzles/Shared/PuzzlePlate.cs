using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Vector3 = UnityEngine.Vector3;

public class PuzzlePlate : MonoBehaviour
{
    [SerializeField] private float playerAlignSpeed;
    [SerializeField] private Vector3 playerAlignRotation;
    public PuzzleData linkedData;
    public BasicPuzzle[] linkedPuzzles;
    [SerializeField] private UnityEvent executeWithPuzzle;
    [SerializeField] private Color activePlateColor, completedPlateColor;
    private Transform playerTr;
    private Material plateMat;
    void Awake()
    {
        plateMat = GetComponent<MeshRenderer>().material;
        linkedData.OnPuzzleCompleted += () => PlateCompleted();
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && GameManager.root.currentState == GameState.Roaming && linkedData.solved < linkedData.solutions.Length)
        {
            PuzzleManager.root.currentPuzzle = linkedData;
            PuzzleManager.root.currentPuzzleBehaviors = linkedPuzzles;
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
            CRManager.root.Begin(AlginPlayer(), $"AlignPlayerTo{gameObject.name}", this);
        }
    }

    IEnumerator AlginPlayer()
    {
        Vector3 playerPos = playerTr.position;
        playerPos.y = transform.position.y;
        while (
            Vector3.Distance(playerPos, transform.position) > 0.25f && 
            Quaternion.Angle(playerTr.rotation, Quaternion.Euler(playerAlignRotation)) > 5f && 
            GameManager.root.currentState == GameState.InPuzzle)
        {
            playerTr.position = Vector3.Lerp(playerTr.position, transform.position + Vector3.up * 1.75f, Time.deltaTime * playerAlignSpeed);
            playerTr.rotation = Quaternion.Lerp(playerTr.rotation, Quaternion.Euler(playerAlignRotation), Time.deltaTime * playerAlignSpeed);
            yield return null;
        }

        playerTr = null;
    }

    void PlateCompleted()
    {
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