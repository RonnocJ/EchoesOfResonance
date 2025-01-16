using System.Collections;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PuzzlePlate : MonoBehaviour
{
    [SerializeField] private float playerAlignSpeed;
    public PuzzleData linkedData;
    public BasicPuzzle[] linkedPuzzles;
    private Transform playerTr;
    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player") && GameManager.root.currentState == GameState.Roaming && linkedData.solved < linkedData.solutions.Length)
        {
            PuzzleManager.root.currentPuzzle = linkedData;
            PuzzleManager.root.currentPuzzleBehaviors = linkedPuzzles;
            GameManager.root.currentState = GameState.InPuzzle;
            playerTr = col.GetComponent<Transform>();
            StartCoroutine(AlginPlayer());
        }
    }

    IEnumerator AlginPlayer()
    {
        Vector3 playerPos = playerTr.position;
        playerPos.y = transform.position.y;
        while(Vector3.Distance(playerPos, transform.position) > 0.25f && GameManager.root.currentState == GameState.InPuzzle)
        {
            playerTr.position = Vector3.Lerp(playerTr.position, transform.position + Vector3.up * 1.75f, Time.deltaTime * playerAlignSpeed);
            yield return null;
        }

        playerTr = null;
    }
}