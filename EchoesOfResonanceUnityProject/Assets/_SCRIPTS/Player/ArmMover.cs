using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmMover : MonoBehaviour, IInputScript, ISaveData
{
    public bool hasBroadcaster;
    [SerializeField] private float _armMoveSpeed;
    [SerializeField] private GameObject _pedestalBroadcaster;
    private int _setControlHash, _playControlHash, _obtainedBroadcasterHash;
    private Animator _armAnim;
    void Awake()
    {
        _armAnim = GetComponent<Animator>();
        _setControlHash = Animator.StringToHash("SetIn");
        _playControlHash = Animator.StringToHash("Playing");
        _obtainedBroadcasterHash = Animator.StringToHash("HasBroadcaster");

        BrBattery.root.OnNotesHeldChange += MoveArmPlaying;
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.Settings, MoveArmSettings);
    }
    public Dictionary<string, object> AddSaveData()
    {
        return new()
        {
            {"hasBroadcaster", hasBroadcaster}
        };
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        if (savedData.TryGetValue("hasBroadcaster", out object data))
        {
            bool didHaveBroadcaster = Convert.ToBoolean(data);
            hasBroadcaster = didHaveBroadcaster;

            _armAnim.SetBool(_obtainedBroadcasterHash, hasBroadcaster);
        }
    }
    public void PickUpBroadcaster()
    {
        _armAnim.SetTrigger("PickUpBroadcaster");
    }
    public void ReParentBroadcaster()
    {
        _pedestalBroadcaster.SetActive(false);
    }
    public void EndCutscene()
    {
        GameManager.root.currentState = GameState.Roaming;

        var resetCamera = new TrData(Vector3.zero, Quaternion.identity);
        CRManager.root.Begin(resetCamera.ApplyToOverTime(Camera.main.transform, 0.5f), "ResetCameraCutscene", this);
        
        hasBroadcaster = true;
    }
    void MoveArmPlaying(int notesHeld)
    {
        _armAnim.SetBool(_playControlHash, notesHeld > 0);
    }

    [AllowedStates(GameState.InPuzzle, GameState.Roaming, GameState.Settings)]
    void MoveArmSettings(float setingsInput)
    {
        CRManager.root.Restart(InterpolateAnimation(_setControlHash, _armAnim.GetFloat(_setControlHash), setingsInput), "MoveArmSettings", this);
    }
    public void StopBroadcasterSounds()
    {
        AudioManager.root.PlaySound(AudioEvent.stopBroadcasterFX, BrBattery.root.gameObject);
    }
    IEnumerator InterpolateAnimation(int param, float beginValue, float endValue)
    {
        if (beginValue < endValue)
        {
            while (_armAnim.GetFloat(param) < endValue)
            {
                _armAnim.SetFloat(param, _armAnim.GetFloat(param) + (Time.deltaTime * _armMoveSpeed));
                yield return null;
            }

            _armAnim.SetFloat(param, endValue);
        }
        else
        {
            while (_armAnim.GetFloat(param) > endValue)
            {
                _armAnim.SetFloat(param, _armAnim.GetFloat(param) - (Time.deltaTime * _armMoveSpeed));
                yield return null;
            }

            _armAnim.SetFloat(param, endValue);
        }
    }
}
