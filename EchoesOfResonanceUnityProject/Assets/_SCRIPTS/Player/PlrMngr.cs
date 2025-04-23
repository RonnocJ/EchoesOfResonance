using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlrMngr : Singleton<PlrMngr>, IInputScript, ISaveData
{
    [HideInInspector]
    public float moveInput, lookInput, sineTime;
    [HideInInspector]
    public TrData savedPosition;
    [SerializeField] private float moveSpeed, lookSpeed, bobIntensity, bobSpeed;
    public UnityEvent onFootstep;
    private bool isGrounded, hasPlayedFootstep;
    private RaycastHit slopeHit;
    private Rigidbody rb;
    private Transform cameraTr;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTr = transform.GetChild(0);
    }
    public Dictionary<string, object> AddSaveData()
    {
        if (GameManager.root.State > GameState.InPuzzle)
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
        if (DH.Get<TestOverrides>().overrideSpawn)
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
        else
        {
            TrData defaultPosition = new TrData(new Vector3(0, 4, -4), Quaternion.identity);
            defaultPosition.ApplyTo(transform);
        }
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.ModwheelChange, UpdateMoveInput);
        InputManager.root.AddListener<float>(ActionTypes.PitchbendChange, UpdateLookInput);
    }
    [AllowAllAboveState(GameState.Intro)]
    void UpdateMoveInput(float modInput)
    {
        moveInput = modInput;
    }
    [AllowAllAboveState(GameState.InPuzzle)]
    void UpdateLookInput(float pitchInput)
    {
        lookInput = pitchInput;
    }
    void Update()
    {
        if (lookInput != 0 && GameManager.root.State >= GameState.InPuzzle)
            transform.localEulerAngles += Vector3.up * lookInput * lookSpeed * Time.deltaTime;

        if (GameManager.root.State >= GameState.Intro && moveInput > 0.05f)
        {
            isGrounded = Physics.CheckSphere(transform.position - transform.up * 4, 1.5f);

            if (isGrounded)
            {
                rb.AddForce(transform.forward * (1 - Mathf.Pow(1 - moveInput, 2.5f)) * moveSpeed * Time.deltaTime, ForceMode.Force);

                if (OnSlope())
                {
                    rb.AddForce(GetSlopeDir() * 0.5f * (1 - Mathf.Pow(1 - moveInput, 2.5f)) * moveSpeed * Time.deltaTime, ForceMode.Force);
                }

                sineTime += Time.deltaTime * bobSpeed * moveInput;

                if (moveInput > 0.33f) cameraTr.localPosition = new Vector3(moveInput * bobIntensity * 0.1f * Mathf.Sin(sineTime), Mathf.Abs(moveInput * bobIntensity * Mathf.Sin(sineTime)), 0);

                AudioManager.root.SetRTPC(AudioRTPC.player_MoveSpeed, Mathf.Pow(moveInput, 3) * 100f);

                if (Mathf.Abs(Mathf.Sin(sineTime)) > 0.75f && !hasPlayedFootstep)
                {
                    AudioManager.root.PlaySound(AudioEvent.playFootsteps, gameObject);
                    hasPlayedFootstep = true;
                }
                else if (Mathf.Abs(Mathf.Sin(sineTime)) < 0.5f && hasPlayedFootstep)
                {
                    hasPlayedFootstep = false;
                }
            }
        }
        else
        {
            moveInput = 0;

            sineTime = (sineTime > 0) ? sineTime - Time.deltaTime * bobSpeed : 0;
            cameraTr.localPosition = Vector3.Lerp(cameraTr.localPosition, Vector3.zero, Time.deltaTime * bobSpeed);
        }
    }
    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 6))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < 37.5f && angle != 0;
        }
        return false;
    }

    Vector3 GetSlopeDir()
    {
        return Vector3.ProjectOnPlane(transform.forward, slopeHit.normal).normalized;
    }
}