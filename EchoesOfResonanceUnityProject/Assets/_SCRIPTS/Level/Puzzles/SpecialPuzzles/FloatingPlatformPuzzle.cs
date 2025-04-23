using System.Collections;
using System.Linq;
using UnityEngine;

public class FloatingPlatformPuzzle : MonoBehaviour, ISpecialPuzzle
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private PuzzleData[] _linkedDatas;
    [SerializeField] private Gem[] _gems;
    [SerializeField] private Transform _displayTr;
    [SerializeField] private bool _active;
    private int _currentPuzzleIndex;
    private PuzzlePlate _connectedPlate;
    private Transform _playerTr;

    private readonly Vector3[] directions = {
        Vector3.forward,
        Vector3.right,
        -Vector3.forward,
        -Vector3.right
    };
    void Awake()
    {
        _connectedPlate = GetComponent<PuzzlePlate>();
        _connectedPlate.linkedData = _linkedDatas[0];
        _connectedPlate.SetupPuzzle();

        _active = false;
        _currentPuzzleIndex = 0;

        for (int i = 0; i < _linkedDatas.Length; i++)
        {
            _linkedDatas[i].reset = 0;
            _linkedDatas[i].OnPuzzleCompleted += PlatformSequenceSolved;
            _linkedDatas[i].OnReset += PlatformReset;
        }
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && GameManager.root.State == GameState.Roaming)
        {
            _playerTr = col.transform;
            _active = true;
        }
    }
    void Update()
    {
        if (GameManager.root.State == GameState.InPuzzle && _active)
        {
            float playerYRot = Mathf.Round((_playerTr.localEulerAngles.y + 360f) % 360 / 90f) * 90;

            if ((int)playerYRot / 90 % 4 != _currentPuzzleIndex)
            {
                _currentPuzzleIndex = (int)playerYRot / 90 % 4;
                _connectedPlate.DeactivatePuzzle();
                _connectedPlate.linkedData = _linkedDatas[_currentPuzzleIndex];
                _connectedPlate.SetupPuzzle();

                _linkedDatas[_currentPuzzleIndex].OnSolvedChanged.Invoke(_linkedDatas[_currentPuzzleIndex].solved);

                for (int i = 0; i < 4; i++)
                {
                    CRManager.root.Restart(
                        _gems[i].ShiftGem(PzUtil.GetNoteNumber(_linkedDatas[_currentPuzzleIndex].solutions[i].noteName), 1.5f, i < _linkedDatas[_currentPuzzleIndex].solved),
                        $"ShiftFloatingPlatformGem{i}", this
                    );
                }

                var target = new TrData(Vector3.up * -0.2f, Quaternion.Euler(0f, playerYRot, 0f));
                CRManager.root.Restart(target.ApplyToOverConstant(_displayTr, 50), "MoveFloatingPlatformDisplay", this);
            }
        }
    }
    void PlatformSequenceSolved()
    {
        for (int j = 0; j < _linkedDatas.Length; j++)
        {
            if (j != _currentPuzzleIndex)
            {
                _linkedDatas[j].solved = 0;
                _linkedDatas[j].reset = 0;
            }
            else
            {
                for (int k = 0; k < _connectedPlate.gems.Length; k++)
                {
                    _connectedPlate.gems[k].gameObject.name = $"Gem{_linkedDatas[j].solutions[k].noteName}_{k}";
                }
                CRManager.root.Restart(MovePlatform(_currentPuzzleIndex), "MoveFloatingPlatform", this);
            }
        }
    }
    IEnumerator MovePlatform(int moveDir)
    {
        while (true)
        {
            transform.parent.position += directions[moveDir] * 7.5f * Time.deltaTime;

            if (Physics.BoxCast(transform.position, _displayTr.GetComponent<MeshCollider>().bounds.extents, directions[moveDir], Quaternion.identity, 1f))
            {
                break;
            }

            yield return null;
        }
    }
    void PlatformReset()
    {
        _active = false;
        CRManager.root.Stop("MoveFloatingPlatform", this);
        foreach(var data in _linkedDatas)
        {
            data.solved = 0;
            data.Active = false;
        }
    }
}