using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmMover : MonoBehaviour, IInputScript, ISaveData
{
    public bool hasBroadcaster;
    [SerializeField] private float _armMoveSpeed;
    private int _setControlHash, _playControlHash, _obtainedBroadcasterHash;
    private Animator _armAnim;
    void Awake()
    {
       _armAnim = GetComponent<Animator>();
       _setControlHash = Animator.StringToHash("SetIn");
       _playControlHash = Animator.StringToHash("Playing");
       _obtainedBroadcasterHash = Animator.StringToHash("HasBroadcaster");
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, MoveArmPlaying);
        InputManager.root.AddListener<float>(ActionTypes.KeyUp, MoveArmIdle);
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
        if(savedData.TryGetValue("hasBroadcaster", out object data))
        {
            bool didHaveBroadcaster = Convert.ToBoolean(data);
            hasBroadcaster = didHaveBroadcaster;

            _armAnim.SetBool(_obtainedBroadcasterHash, hasBroadcaster);
        }
    }
    public void ReParentBroadcaster()
    {
        BrBattery.root.transform.parent = transform.GetChild(1);
    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming)]
    void MoveArmPlaying(float noteInput)
    {
        if(BrBattery.root.notesHeld > 0)
            _armAnim.SetBool(_playControlHash, true);
    }
    void MoveArmIdle(float noteInput)
    {
        if(BrBattery.root.notesHeld == 0)
            _armAnim.SetBool(_playControlHash, false);
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
        if(beginValue < endValue)
        {
            while(_armAnim.GetFloat(param) < endValue)
            {
                _armAnim.SetFloat(param, _armAnim.GetFloat(param) + (Time.deltaTime * _armMoveSpeed));
                yield return null;
            }

            _armAnim.SetFloat(param, endValue);
        }
        else
        {
            while(_armAnim.GetFloat(param) > endValue)
            {
                _armAnim.SetFloat(param, _armAnim.GetFloat(param) - (Time.deltaTime * _armMoveSpeed));
                yield return null;
            }

            _armAnim.SetFloat(param, endValue);
        }
    }
}
