using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : Singleton<PlayerManager>, IInputScript, ISaveData
{
    [HideInInspector]
    public float moveInput, lookInput, sineTime;
    [HideInInspector]
    public TrData savedPosition;
    [SerializeField] private float moveSpeed, lookSpeed, bobIntensity, bobSpeed;
    public UnityEvent onFootstep;
    private bool isGrounded, hasPlayedFootstep;
    private RaycastHit groundHit;
    private Rigidbody rb;
    private Transform cameraTr;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTr = transform.GetChild(0);
    }
    public Dictionary<string, object> AddSaveData()
    {
        if (GameManager.root.currentState != GameState.InPuzzle)
        {
            return new()
            {
                {"playerPos", new SaveStruct(new TrData(transform, TrData.IncludeInMove.Position | TrData.IncludeInMove.Rotation | TrData.IncludeInMove.Scale))}
            };
        }
        else
        {
            return new()
            {
                {"playerPos", new SaveStruct(savedPosition)}
            };
        }
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        if(DH.Get<TestOverrides>().overrideSpawn)
        {
            TrData overridePosition = new TrData(DH.Get<TestOverrides>().playerSpawnPosition);
            overridePosition.ApplyTo(transform);
        }
        else if (savedData.TryGetValue("playerPos", out object oldSavedRaw))
        {
            string json = JsonConvert.SerializeObject(oldSavedRaw);
            SaveStruct oldSaved = JsonConvert.DeserializeObject<SaveStruct>(json);

            TrData oldSavedPosition = oldSaved.LoadData();
            oldSavedPosition.ApplyTo(transform);
        }
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.ModwheelChange, UpdateMoveInput);
        InputManager.root.AddListener<float>(ActionTypes.PitchbendChange, UpdateLookInput);
    }


    [AllowedStates(GameState.Roaming, GameState.Shutdown)]
    void UpdateMoveInput(float modInput)
    {
        moveInput = modInput;
    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming, GameState.Shutdown)]
    void UpdateLookInput(float pitchInput)
    {
        lookInput = pitchInput;
    }
    void Update()
    {
        if (lookInput != 0)
            transform.localEulerAngles += Vector3.up * lookInput * (InputManager.root.UsingMidiKeyboard ? lookSpeed : lookSpeed / 2f) * Time.deltaTime;

        if (GameManager.root.currentState is GameState.Roaming or GameState.Shutdown && moveInput > 0.2f)
        {
            isGrounded = Physics.CheckSphere(transform.position - transform.up * 4, 1.5f);

            if (isGrounded)
            {
                sineTime += Time.deltaTime * bobSpeed;

                rb.AddForce(moveInput * moveSpeed * Time.deltaTime * (OnSlope() ? Vector3.ProjectOnPlane(transform.forward, groundHit.normal).normalized * 1.5f : transform.forward.normalized), ForceMode.Acceleration);
                cameraTr.localPosition = new Vector3(0, Mathf.Abs(moveInput * bobIntensity * Mathf.Sin(sineTime)), 0);

                if (transform.GetChild(0).localPosition.y < 0.05f && !hasPlayedFootstep)
                {
                    AudioManager.root.PlaySound(AudioEvent.playFootsteps, gameObject);
                    onFootstep.Invoke();
                    hasPlayedFootstep = true;
                }
                else if (transform.GetChild(0).localPosition.y > 0.05f)
                {
                    hasPlayedFootstep = false;
                }
            }
        }
        else
        {
            moveInput = 0f;
        }
    }
    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit groundHit, 4))
        {
            if (groundHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}