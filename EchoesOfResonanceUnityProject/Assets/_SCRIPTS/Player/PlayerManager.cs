using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : Singleton<PlayerManager>, IInputScript
{
    [SerializeField] private float moveSpeed, lookSpeed, bobIntensity, bobSpeed;
    [SerializeField] private AK.Wwise.Event footstepSound;
    private bool hasPlayedFootstep;
    public float moveInput, lookInput, sineTime, baseMoveSpeed;
    private Rigidbody rb;
    private Transform cameraTr, broadcasterTr;
    void Start()
    {
        baseMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
        cameraTr = transform.GetChild(0);
        broadcasterTr = cameraTr.GetChild(0);
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.ModwheelChange, UpdateMoveInput);
        InputManager.root.AddListener<float>(ActionTypes.PitchbendChange, UpdateLookInput);
    }
    [AllowedStates(GameState.Roaming)]
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
        if (GameManager.root.currentState != GameState.Final)
        {
            if (lookInput != 0)
                transform.localEulerAngles += Vector3.up * lookInput * (InputManager.root.usingMidiKeyboard? lookSpeed : lookSpeed / 2f) * Time.deltaTime;
    
            if (GameManager.root.currentState == GameState.Roaming && moveInput > 0.2f)
            {
                sineTime += Time.deltaTime * bobSpeed * moveInput;
                rb.AddForce(transform.forward * moveInput * moveSpeed * Time.deltaTime);
                cameraTr.localPosition = new Vector3(0, Mathf.Abs(moveInput * bobIntensity * Mathf.Sin(sineTime)), 0);
                broadcasterTr.localPosition = new Vector3(broadcasterTr.localPosition.x, -0.38f + -Mathf.Abs(moveInput * 0.5f * bobIntensity * Mathf.Sin(sineTime)), broadcasterTr.localPosition.z);
    
                if (transform.GetChild(0).localPosition.y < 0.05f && !hasPlayedFootstep)
                {
                    footstepSound.Post(gameObject);
                    hasPlayedFootstep = true;
                }
                else if (transform.GetChild(0).localPosition.y > 0.05f)
                {
                    hasPlayedFootstep = false;
                }
            }
            else
            {
                moveInput = 0f;
                broadcasterTr.localPosition = Vector3.Lerp(broadcasterTr.localPosition, new Vector3(broadcasterTr.localPosition.x, -0.38f, broadcasterTr.localPosition.z), Time.deltaTime * 4f);
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.collider.name.Contains("Ramp"))
        {
            moveSpeed = baseMoveSpeed * 2f;
        }
        else if(moveSpeed != baseMoveSpeed)
        {
            moveSpeed = baseMoveSpeed;
        }
    }
}