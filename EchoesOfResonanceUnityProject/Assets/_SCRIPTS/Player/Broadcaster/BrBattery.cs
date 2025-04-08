using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrBattery : Singleton<BrBattery>
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
                    CRManager.root.Begin(ShutdownRoutine(), "Shutdown", this);
                }

                _batteryLevel = clampedValue;
            }
        }
    }
    private int _notesHeld;
    public event Action<int> OnNotesHeldChange;
    [HideInInspector]
    public int notesHeld
    {
        get => _notesHeld;
        set
        {
            if(_notesHeld != value)
            {
                _notesHeld = value;
                OnNotesHeldChange.Invoke(_notesHeld);
            }
        }
    }

public Image batteryMeter;
public Image infoScreen;
public TextMeshProUGUI noteInfoText;
public Camera cam;


    void Start()
    {
        batteryLevel = 1f;
        OnBatteryUpdate += () => UpdateBatteryLevel();
    }

    public IEnumerator DrainBatteryRoutine(float amount)
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
            batteryLevel += regenMultiplier * 0.05f * Time.deltaTime;
            regenMultiplier *= 1.005f;
            yield return null;
        }
    }
    IEnumerator ShutdownRoutine()
    {
        GameManager.root.currentState = GameState.Shutdown;
        OnBatteryEmpty?.Invoke();
        
        batteryMeter.fillAmount = 0;

        AudioManager.root.StopSound(AudioEvent.playBroadcasterFX, gameObject);
        AudioManager.root.PlaySound(AudioEvent.playShutoff, gameObject);
        AudioManager.root.SetRTPC(AudioRTPC.broadcaster_Shutdown, 100);

        CRManager.root.Stop("Regen", this);
        CRManager.root.Stop("DrainBatteryNote", this);

        infoScreen.color = Color.black;

        yield return new WaitForSeconds(7.5f);

        AudioManager.root.SetRTPC(AudioRTPC.broadcaster_Shutdown, 0);

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

    void OnDisable()
    {
        OnBatteryUpdate -= () => UpdateBatteryLevel();
        OnBatteryEmpty -= () => CRManager.root.Begin(ShutdownRoutine(), "Shutdown", this);
    }
}