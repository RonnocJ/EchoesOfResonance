using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum DisplayPriority
{
    Default = 0,
    Sync = 1,
    Finder = 2,
    Playing = 3,
    Settings = 4
}
public class BrDisplay : Broadcaster
{
    [SerializeField] private Color bgcolor;
    [SerializeField] private GameObject screen;
    [SerializeField] private GameObject backLight;
    [SerializeField] private Image batteryMeter;
    [SerializeField] private TextMeshProUGUI gameInfoText;
    private Dictionary<DisplayPriority, string> _activePriority = new();
    public DisplayPriority _currentPriority;
    private GameState oldState;
    public override void Awake()
    {
        RegisterActiveBroadcaster(this);
        for (int i = 0; i < 5; i++)
        {
            _activePriority[(DisplayPriority)i] = "";
        }

        OnHeldNotesEmptied += DisplayModText;
        OnBatteryChange += UpdateBatteryLevel;
        OnBatteryEmpty += DisplayOff;
        OnBatteryCharge += DisplayOn;

        DefaultText();
    }
    public override void OnPuzzleSynced()
    {
        SetSyncText(activePuzzle.solutions[0].note.Name);
    }
    public override void OnPuzzleEnter()
    {
        LowerTextPriority(DisplayPriority.Sync);
    }
    public override void OnPuzzleExit()
    {
        LowerTextPriority(DisplayPriority.Sync);
    }
    public void DisplayOn()
    {
        CRManager.Begin(UIUtil.root.FadeToColor(0.5f, 0, bgcolor, new() { screen, backLight }), "BrScreenToGreen", this);
    }
    public void DisplayOff()
    {
        CRManager.Begin(UIUtil.root.FadeToColor(0.25f, 0, Color.black, new() { screen, backLight }), "BrDisplayOff", this);
    }
    public void UpdateBatteryLevel(float changeAmount)
    {
        batteryMeter.fillAmount = Mathf.Clamp(batteryMeter.fillAmount + changeAmount, 0, 1);
    }
    void DisplayText(string newText, DisplayPriority newPriority)
    {
        _activePriority[newPriority] = newText;

        if (newPriority >= _currentPriority)
        {
            switch (newPriority)
            {
                case DisplayPriority.Playing:
                    CRManager.Restart(AnimateText(newText), "BrTextAnim", this);
                    break;
                case DisplayPriority.Finder:
                    if (_currentPriority != newPriority)
                    {
                        CRManager.Restart(AnimateText(newText), "BrTextAnim", this);
                    }
                    else
                    {
                        gameInfoText.text = newText;
                    }
                    break;
                default:
                    CRManager.Restart(AnimateText(newText), "BrTextAnim", this);
                    break;
            }

            _currentPriority = newPriority;
        }
    }
    private IEnumerator AnimateText(string newText)
    {
        int matchIndex = 0;
        string oldText = gameInfoText.text;

        while (matchIndex < oldText.Length && matchIndex < newText.Length && oldText[matchIndex] == newText[matchIndex])
        {
            matchIndex++;
        }

        for (int i = oldText.Length; i > matchIndex; i--)
        {
            gameInfoText.text = oldText.Substring(0, i - 1);

            if (i % 2 == 0)
                yield return new WaitForSeconds(0.01f);
        }

        for (int i = matchIndex; i < newText.Length; i++)
        {
            gameInfoText.text = newText.Substring(0, i);

            if (!(oldText.Contains("Estimated") && newText.Contains("Estimated"))
                && !(oldText.Contains("Playing") && newText.Contains("Playing"))
                && GameManager.root.State != GameState.Shutdown
                && (newText[i] == '\n' || i == newText.Length - 1))
            {
                AudioManager.root.PlaySound(AudioEvent.playTextBeep, gameObject);
                yield return new WaitForSeconds(0.05f);
            }

        }

        gameInfoText.text = newText;
    }
    public void LowerTextPriority(DisplayPriority priorityToWipe)
    {
        _activePriority[priorityToWipe] = "";

        if (priorityToWipe >= _currentPriority)
        {
            for (int i = (int)priorityToWipe; i >= 0; i--)
            {
                if (_activePriority[(DisplayPriority)i] != "" && i > 0)
                {
                    _currentPriority = (DisplayPriority)i;
                    DisplayText(_activePriority[(DisplayPriority)i], (DisplayPriority)i);
                }
                else if (i == 0)
                {
                    _currentPriority = DisplayPriority.Default;
                    DefaultText();
                }
            }
        }
    }
    public void DefaultText()
    {
        switch (GameManager.root.State)
        {
            case GameState.InPuzzle:
                DisplayText("Connected to Sync Plate \n Play C2 3 times \n to disconnect", DisplayPriority.Default);
                break;
            default:
                DisplayText("Disconnected \n Please locate a \n Sync Plate", DisplayPriority.Default);
                break;
        }
    }

    public override void ModChange(float modInput)
    {
        DisplayModText();
    }
    private void DisplayModText()
    {
        if (modInput > 0.2f)
            DisplayText($"Estimated Resonance: \n {finderEstimate.Name}", DisplayPriority.Finder);
        else
            LowerTextPriority(DisplayPriority.Finder);
    }
    public override void OnNoteOn(int newNote, int velocity)
    {
        string textToSet = "Playing: \n";

        int i = 0;

        foreach (var note in heldNotes)
        {
            textToSet += $"{note.Name}";

            i++;
            if (i == heldNotes.Count) break;
            textToSet += " & ";
        }

        DisplayText(textToSet, DisplayPriority.Playing);
    }
    public void SetSyncText(string startNote)
    {
        DisplayText($"Sync Plate detected \n Play {startNote} to connect", DisplayPriority.Sync);
    }
    [AllowAllAboveState(GameState.Settings), DissallowedStates(GameState.Intro)]
    public override void SettingsChange(float newPauseAmount)
    {
        if (newPauseAmount < 0.5f && oldState != GameState.Settings)
        {
            GameManager.root.State = oldState;

            AudioManager.root.PlaySound(AudioEvent.resumeAll);
        }
        else if (newPauseAmount > 0.5f && GameManager.root.State >= GameState.InPuzzle)
        {
            oldState = GameManager.root.State;
            GameManager.root.State = GameState.Settings;

            AudioManager.root.PlaySound(AudioEvent.pauseAll);

            PlrMngr.root.savedPosition = new TrData(PlrMngr.root.transform);
            PlrMngr.root.lookInput = 0;
            PlrMngr.root.moveInput = 0;
        }
    }

    public override void OnDestroy()
    {
        OnHeldNotesEmptied -= DisplayModText;
        OnBatteryChange -= UpdateBatteryLevel;
        OnBatteryEmpty -= DisplayOff;
        OnBatteryCharge -= DisplayOn;
    }
}
