using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Broadcaster : Singleton<Broadcaster>, IInputScript
{
    private float _batteryLevel = 1f;
    public event Action OnBatteryEmpty, OnBatteryUpdate;

    public float batteryLevel
    {
        get => _batteryLevel;
        set
        {
            if (_batteryLevel != value)
            {
                if (_batteryLevel > value)
                {
                    CRManager.root.Restart(RegenRoutine(), "Regen", this);
                }

                _batteryLevel = value;
                float clampedValue = Mathf.Clamp(value, 0, 1);

                OnBatteryUpdate.Invoke();

                if (_batteryLevel < clampedValue)
                {
                    OnBatteryEmpty?.Invoke();
                }

                _batteryLevel = clampedValue;
            }

        }
    }
    public int sineResolution, laserResolution;
    [HideInInspector]
    public int notesHeld;
    [SerializeField] private float notePlayDrainAmount, noteSustainDrainAmount, gemFinderSpeedScale, gemFinderMinSpeed;
    [SerializeField] private TextMeshProUGUI noteInfoText;
    [SerializeField] private RawImage infoScreen;
    [SerializeField] private Image batteryMeter;
    [SerializeField] private LineRenderer sineWave, gemLaser;
    [SerializeField] private ParticleSystem gemLaserParticle;
    [SerializeField] private AK.Wwise.Event playInstrumentFX, stopInstrumentFX, playShutoff;
    private float frequency, speed, lastNotePlayed, modInput, gemNoteNumber;
    private string gemNoteName;
    private Camera cam;
    public Transform gemTarget;

    void Start()
    {
        batteryLevel = 1f;
        OnBatteryUpdate += () => UpdateBatteryLevel();
        OnBatteryEmpty += () => CRManager.root.Begin(ShutdownRoutine(), "Shutdown", this);
        lastNotePlayed = 13;
        sineWave.positionCount = sineResolution;
        gemLaser.positionCount = laserResolution;
        cam = Camera.main;
    }

    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, SetLastNote);
        InputManager.root.AddListener<float>(ActionTypes.KeyUp, RemoveNote);
        InputManager.root.AddListener<float>(ActionTypes.ModwheelChange, AdjustModValue);
        InputManager.root.AddListener<float>(ActionTypes.PitchbendChange, UpdateGemFinder);

    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming)]
    void SetLastNote(float newNote)
    {
        batteryLevel -= notePlayDrainAmount;

        if (notesHeld == 0)
        {
            playInstrumentFX.Post(gameObject);
        }

        notesHeld++;
        lastNotePlayed = newNote;
        frequency = 2 + (lastNotePlayed * 0.32f);
        speed = 1 + (lastNotePlayed * 0.36f);

        noteInfoText.text = PuzzleUtilities.root.GetNoteName(lastNotePlayed);

        CRManager.root.Restart(DrainBatteryRoutine(noteSustainDrainAmount * notesHeld), "DrainBatteryNote", this);
    }

    [AllowedStates(GameState.InPuzzle, GameState.Roaming)]
    public void RemoveNote(float oldNote)
    {
        if (notesHeld > 0)
        {
            notesHeld--;
        }

        if (notesHeld == 0)
        {
            noteInfoText.text = "";
            CRManager.root.Stop("DrainBatteryNote", this);
            stopInstrumentFX.Post(gameObject);
        }
    }

    [AllowedStates(GameState.InPuzzle, GameState.Roaming)]
    public void AdjustModValue(float modWheelAmount)
    {
        if (modWheelAmount > 0.2f && GameManager.root.currentState == GameState.InPuzzle)
        {
            modInput = modWheelAmount;

            UpdateGemFinder(0);
        }
        else
        {
            modInput = 0;
            DisableFinder();
        }
    }

    [AllowedStates(GameState.InPuzzle)]
    void UpdateGemFinder(float newPitch)
    {
        if (modInput > 0.2f)
        {
            List<Gem> currentGems = GameManager.root.currentPlate.gems.ToList();

            if (gemTarget == null || newPitch != 0f)
            {
                gemTarget = FindClosestGem(cam.transform.position, currentGems);

                gemLaser.enabled = true;
                gemLaserParticle.Play();

                if (notesHeld == 0 && gemNoteName != GameManager.root.currentPuzzle.solutions[gemTarget.GetSiblingIndex()])
                {
                    gemNoteName = GameManager.root.currentPuzzle.solutions[gemTarget.GetSiblingIndex()];
                    gemNoteNumber = PuzzleUtilities.root.GetNoteNumber(gemNoteName);
                    CRManager.root.Begin(FindGemRoutine(), "FindGem", this);
                    CRManager.root.Begin(DrainBatteryRoutine(modInput * 0.5f), "DrainBatteryFinder", this);
                }
                else if (notesHeld > 0 && noteInfoText.text != PuzzleUtilities.root.GetNoteName(lastNotePlayed))
                {
                    noteInfoText.text = PuzzleUtilities.root.GetNoteName(lastNotePlayed);
                }
            }
        }
        else if (gemTarget != null)
        {
            DisableFinder();

            noteInfoText.text = (notesHeld > 0) ? PuzzleUtilities.root.GetNoteName(lastNotePlayed) : "";
        }
    }
    Transform FindClosestGem(Vector3 position, List<Gem> gems)
    {
        return gems
            .OrderBy(gem => Vector3.Angle(gem.transform.position - position, cam.transform.forward))
            .FirstOrDefault()?.transform;
    }
    void DisableFinder()
    {
        Vector3[] positions = Enumerable.Repeat(Vector3.zero, laserResolution).ToArray();
        gemLaser.SetPositions(positions);
        gemLaser.enabled = false;
        gemLaserParticle.Stop();

        gemTarget = null;

        CRManager.root.Stop("FindGem", this);
        CRManager.root.Stop("DrainBatteryFinder", this);

        gemNoteNumber = -1;
        gemNoteName = "";
    }
    IEnumerator FindGemRoutine()
    {
        while (modInput > 0.2f)
        {
            if (modInput == 1)
            {
                noteInfoText.text = gemNoteName;
                yield return new WaitForSeconds(gemFinderMinSpeed);
            }
            else
            {
                float fakeNoteNumber = Mathf.Round(UnityEngine.Random.Range(
                    gemNoteNumber - (6 * Mathf.Abs(1 - modInput)),
                    gemNoteNumber + (6 * Mathf.Abs(1 - modInput))
                ));
                fakeNoteNumber = Mathf.Clamp(fakeNoteNumber, 1, 25);
                noteInfoText.text = PuzzleUtilities.root.GetNoteName(fakeNoteNumber);
                yield return new WaitForSeconds(gemFinderMinSpeed + (Mathf.Abs(1 - modInput) * gemFinderSpeedScale));
            }
        }
    }
    IEnumerator DrainBatteryRoutine(float amount)
    {
        while (batteryLevel > 0f)
        {
            batteryLevel -= amount * Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator RegenRoutine()
    {
        float regenMultiplier = 1;
        yield return new WaitForSeconds(0.75f);
        while (batteryLevel < 1f)
        {
            batteryLevel += regenMultiplier * 0.75f * noteSustainDrainAmount * Time.deltaTime;
            regenMultiplier *= 1.005f;
            yield return null;
        }
    }
    IEnumerator ShutdownRoutine()
    {
        GameManager.root.currentState = GameState.Shutdown;

        batteryMeter.fillAmount = 0;
        NoteManager.root.AllNotesOff();
        stopInstrumentFX.Post(gameObject);
        playShutoff.Post(gameObject);

        CRManager.root.Stop("Regen", this);
        CRManager.root.Stop("DrainBatteryNote", this);

        DisableFinder();

        infoScreen.color = Color.black;

        yield return new WaitForSeconds(5f);

        notesHeld = 0;
        batteryLevel = 1f;
        infoScreen.color = new Color(0.3f, 0.45f, 0.225f, 1);

        for (int i = 0; i < 50; i++)
        {
            batteryMeter.fillAmount += 0.02f;
            yield return null;
        }

        Ray ray = new Ray(cam.transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetComponent<PuzzlePlate>()?.linkedData.solved < hit.collider.GetComponent<PuzzlePlate>()?.linkedData.solutions.Length)
        {
            GameManager.root.currentState = GameState.InPuzzle;
        }
        else
        {
            GameManager.root.currentState = GameState.Roaming;
        }

    }
    public void UpdateBatteryLevel()
    {
        batteryMeter.fillAmount = Mathf.SmoothStep(batteryMeter.fillAmount, batteryLevel, Time.deltaTime * 10f);
    }
    public void DeactivateAnimator()
    {
        GetComponent<Animator>().enabled = false;
    }
    void Update()
    {
        if (notesHeld > 0 || gemNoteNumber != -1)
        {
            if (sineWave.positionCount != sineResolution)
            {
                sineWave.positionCount = sineResolution;
            }

            for (int i = 0; i < sineResolution; i++)
            {
                float x = (i * 2f / sineResolution) - 1f;
                float y = 0.1f * Mathf.Sin((frequency * x) + (Time.timeSinceLevelLoad * speed));
                sineWave.SetPosition(i, new Vector3(x / 2f, y, 0));
            }
        }

        if (gemLaser.enabled)
        {
            for (int i = 0; i < laserResolution; i++)
            {
                Vector3 pos = Vector3.Lerp(gemLaser.transform.position, gemTarget.position, (float)i / laserResolution);
                gemLaser.SetPosition(i, (i > 0) ? (UnityEngine.Random.insideUnitSphere * Mathf.Abs(1 - modInput) * 0.25f) + pos : pos);
            }

            if(GameManager.root.currentState == GameState.Roaming) AdjustModValue(0);
        }
    }

    void OnDestroy()
    {
        OnBatteryUpdate -= () => UpdateBatteryLevel();
        OnBatteryEmpty -= () => CRManager.root.Begin(ShutdownRoutine(), "Shutdown", this);
    }
}