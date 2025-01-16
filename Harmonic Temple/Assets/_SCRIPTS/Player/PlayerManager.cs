using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : Singleton<PlayerManager>, IInputScript
{
    static public float batteryLevel
    {
        get => _batteryLevel;
        set
        {
            if (_batteryLevel != value)
            {
                if (_batteryLevel > value && root.regenRoutine != null) root.StopCoroutine(root.regenRoutine);

                _batteryLevel = value;
                _batteryLevel = Mathf.Clamp(_batteryLevel, 0, 1);
                root.batteryMeter.fillAmount = Mathf.SmoothStep(root.batteryMeter.fillAmount, _batteryLevel, Time.deltaTime * 20f);

                if (_batteryLevel == 0 && root.shutdownRoutine == null) root.shutdownRoutine = root.StartCoroutine(root.ShutdownRoutine());
            }
        }
    }
    private static float _batteryLevel;

    [SerializeField] private float moveSpeed, lookSpeed, notePlayDrainAmount, noteSustainDrainAmount;
    [SerializeField] private TextMeshProUGUI noteText;
    [SerializeField] private Image batteryMeter;
    public int notesHeld;
    private float moveInput, lookInput;
    private Rigidbody rb;
    private Coroutine regenRoutine, shutdownRoutine;
    void Start()
    {
        batteryLevel = 1;
        rb = GetComponent<Rigidbody>();
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, _ =>
        {
            if (GameManager.root.currentState != GameState.Config) batteryLevel -= notePlayDrainAmount;
            notesHeld++;
        });

        InputManager.root.AddListener<float>(ActionTypes.KeyUp, _ =>
        {
            notesHeld--;
            if (notesHeld == 0) regenRoutine = StartCoroutine(RegenRoutine());
        });

        InputManager.root.AddListener<float>(ActionTypes.ModwheelChange, input => input = moveInput);
        InputManager.root.AddListener<float>(ActionTypes.PitchbendChange, input => input = lookInput);
    }
    IEnumerator RegenRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        while (batteryLevel < 1f)
        {
            batteryLevel += 0.75f * noteSustainDrainAmount * Time.deltaTime;
            yield return null;
        }
        regenRoutine = null;
    }
    IEnumerator ShutdownRoutine()
    {
        GameManager.root.currentState = GameState.Shutdown;

        batteryMeter.fillAmount = 0;
        NoteManager.root.AllNotesOff();

        yield return new WaitForSeconds(5f);

        notesHeld = 0;
        root.shutdownRoutine = null;
        batteryLevel = 1f;

        for(int i = 0; i < 50; i++)
        {
            batteryMeter.fillAmount += 0.02f;
            yield return null;
        }

        GameManager.root.currentState = GameState.Roaming;
    }
    void Update()
    {
        if (GameManager.root.currentState != GameState.Config)
        {
            transform.localEulerAngles += Vector3.up * lookInput * lookSpeed * Time.deltaTime;

            if (GameManager.root.currentState == GameState.Roaming)
            {
                rb.AddForce(transform.forward * moveInput * moveSpeed * Time.deltaTime);
            }

            if (notesHeld > 0 && GameManager.root.currentState != GameState.Shutdown)
            {
                batteryLevel -= noteSustainDrainAmount * notesHeld * Time.deltaTime;
            }
        }
    }
}