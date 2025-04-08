using UnityEngine;

public class FloatingPlatformPuzzle : MonoBehaviour, ISpecialPuzzle
{
    [SerializeField] private PuzzleData[] _linkedDatas;
    [SerializeField] private Gem[] _gems;
    [SerializeField] private Transform _displayTr;
    [SerializeField] private bool _active;
    private int _currentPuzzleIndex;
    private PuzzlePlate _connectedPlate;
    private Transform _playerTr;
    void Awake()
    {
        _connectedPlate = GetComponent<PuzzlePlate>();
        _connectedPlate.linkedData = _linkedDatas[0];
        _connectedPlate.SetupPuzzle();

        _active = false;
        _currentPuzzleIndex = 0;
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && GameManager.root.currentState == GameState.Roaming)
        {
            _playerTr = col.transform;
            _active = true;
        }
    }

    void Update()
    {
        if (GameManager.root.currentState == GameState.InPuzzle && _active)
        {
            if (Mathf.FloorToInt(_playerTr.localEulerAngles.y / 90) != _currentPuzzleIndex)
            {
                _currentPuzzleIndex = Mathf.FloorToInt(_displayTr.localEulerAngles.y / 90);
                _connectedPlate.DeactivatePuzzle();
                _connectedPlate.linkedData = _linkedDatas[_currentPuzzleIndex];
                _connectedPlate.SetupPuzzle();

               var target = new TrData(Vector3.up * -0.2f, Quaternion.Euler(0f, Mathf.Round((_playerTr.localEulerAngles.y + 45f) / 90f) * 90 % 360f, 0f)); 
               CRManager.root.Restart(target.ApplyToOverConstant(_displayTr, 20), "MoveFloatingPlatformDisplay", this);
            }
        }
    }
}