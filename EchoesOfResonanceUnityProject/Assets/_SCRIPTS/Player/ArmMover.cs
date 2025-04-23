using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmMover : MonoBehaviour, IInputScript, ISaveData
{
    static public bool HasBroadcaster;
    [SerializeField] private float _armMoveSpeed;
    [SerializeField] private Transform _fakeBroadcaster;
    private int _setControlHash, _playControlHash, _obtainedBroadcasterHash;
    private Animator _armAnim;
    void Awake()
    {
        _armAnim = GetComponent<Animator>();
        _setControlHash = Animator.StringToHash("SetIn");
        _playControlHash = Animator.StringToHash("Playing");
        _obtainedBroadcasterHash = Animator.StringToHash("HasBroadcaster");

        BrBattery.root.OnNotesHeldChange += CheckNotes;
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.Settings, MoveArmSettings);
    }
    public Dictionary<string, object> AddSaveData()
    {
        return new()
        {
            {"hasBroadcaster", HasBroadcaster}
        };
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        HasBroadcaster = false;

        if (savedData.TryGetValue("hasBroadcaster", out object data))
        {
            bool didHaveBroadcaster = Convert.ToBoolean(data);
            HasBroadcaster = didHaveBroadcaster;
        }

        if (!HasBroadcaster)
        {
            BrBattery.root.BackgroundScreen.GetComponent<Image>().color = Color.black;
            BrBattery.root.BackLight.GetComponent<Light>().color = Color.black;
        }

        _armAnim.SetBool(_obtainedBroadcasterHash, HasBroadcaster);
    }
    public void PickUpBroadcaster()
    {
        _armAnim.SetTrigger("PickUpBroadcaster");
    }
    public void ReParentBroadcaster()
    {
        _fakeBroadcaster.localScale = Vector3.zero;
    }
    public void EndCutscene()
    {
        GameManager.root.State = GameState.Roaming;

        var br = BrBattery.root;
        CRManager.root.Begin(UIUtil.root.FadeToColor(0.5f, 0, br.DefaultBgColor, new() { br.BackgroundScreen, br.BackLight }), "BrScreenToGreen", br);

        var resetCamera = new TrData(Vector3.zero, Quaternion.identity);
        CRManager.root.Begin(resetCamera.ApplyToOverTime(Camera.main.transform, 0.5f), "ResetCameraCutscene", this);
        
        HasBroadcaster = true;
    }
    void CheckNotes(int notesHeld)
    {
        CRManager.root.Restart(MoveArmPlaying(notesHeld), "ReturnArmIdle", this);
    }
    IEnumerator MoveArmPlaying(int notesHeld)
    {
        if(notesHeld > 0)
        {
            _armAnim.SetBool(_playControlHash, true);
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
            _armAnim.SetBool(_playControlHash, false);
        }       
    }

    [AllowAllAboveState(GameState.Settings), DissallowedStates(GameState.Intro, GameState.Shutdown)]
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
