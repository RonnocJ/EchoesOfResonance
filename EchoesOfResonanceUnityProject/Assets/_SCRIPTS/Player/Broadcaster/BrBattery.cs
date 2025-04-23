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
                    OnBatteryEmpty?.Invoke();
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
            if (_notesHeld != value)
            {
                _notesHeld = value;
                OnNotesHeldChange.Invoke(_notesHeld);

                CRManager.root.Restart(DrainBatteryRoutine(noteSustainDrainAmount * _notesHeld), "DrainBatteryNote", this);

                if(value > _notesHeld)
                {
                    batteryLevel -= notePlayDrainAmount;
                }
                else if(value == 0)
                {
                    CRManager.root.Stop("DrainBatteryNote", this);
                }
            }
        }
    }
    [SerializeField] private float notePlayDrainAmount, noteSustainDrainAmount;
    public Color DefaultBgColor;
    [SerializeField] private Image batteryMeter;
    public GameObject BackgroundScreen;
    public GameObject BackLight;
    [SerializeField] private TextMeshProUGUI noteInfoText;
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
        GameManager.root.State = GameState.Shutdown;

        InputManager.root.AllNotesOff();

        batteryMeter.fillAmount = 0;

        AudioManager.root.StopSound(AudioEvent.playBroadcasterFX, gameObject, 1);
        AudioManager.root.PlaySound(AudioEvent.playShutoff, gameObject);
        AudioManager.root.SetRTPC(AudioRTPC.broadcaster_Shutdown, 100);

        CRManager.root.Stop("Regen", this);
        CRManager.root.Stop("DrainBatteryNote", this);

        CRManager.root.Begin(UIUtil.root.FadeToColor(0.2f, 0, Color.black, new() { BackgroundScreen, BackLight }), "BrScreenToBlack", this);

        yield return new WaitForSeconds(5f);

        AudioManager.root.SetRTPC(AudioRTPC.broadcaster_Shutdown, 0);

        batteryLevel = 1f;
        
        CRManager.root.Begin(UIUtil.root.FadeToColor(0.1f, 0, DefaultBgColor, new() { BackgroundScreen, BackLight }), "BrScreenToGreen", this);

        for (int i = 0; i < 50; i++)
        {
            batteryMeter.fillAmount += 0.02f;
            yield return null;
        }
        RaycastHit[] hits = Physics.SphereCastAll(PlrMngr.root.transform.position, 1f, Vector3.down, 4f);

        foreach (var hit in hits)
        {
            if (hit.collider.GetComponent<PuzzlePlate>())
            {
                GameManager.root.State = GameState.Synced;
                break;
            }
        }

        if (GameManager.root.State == GameState.Shutdown)
        {
            GameManager.root.State = GameState.Roaming;
        }
    }
    public void UpdateBatteryLevel()
    {
        batteryMeter.fillAmount = Mathf.SmoothStep(batteryMeter.fillAmount, batteryLevel, Time.deltaTime * 10f);
    }

    void OnDisable()
    {
        OnBatteryUpdate -= UpdateBatteryLevel;
    }
}