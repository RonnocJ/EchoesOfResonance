using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public enum DisplayPriority
{
    Default = 0,
    Finder = 1,
    Playing = 2,
    Sync = 3,
    Settings = 4
}
public class BrDisplay : Singleton<BrDisplay>, IInputScript, ISaveData
{
    public TextMeshProUGUI GameInfoText;
    private Dictionary<DisplayPriority, string> _activePriority = new();
    public DisplayPriority _currentPriority;
    [HideInInspector]
    public float middleKey, tempMiddleKey;
    private GameState oldState;
    private List<string> _heldNotes = new();
    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            _activePriority[(DisplayPriority)i] = "";
        }

        DefaultText();
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.Settings, OpenSettings);
    }
    public Dictionary<string, object> AddSaveData()
    {
        return new Dictionary<string, object>
        {
            {"middleKey", middleKey},
        };
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        this.middleKey = 60f;

        if (savedData.TryGetValue("middleKey", out object middleKey))
        {
            this.middleKey = Convert.ToSingle(middleKey);
        }
    }
    void DisplayText(string newText, DisplayPriority newPriority)
    {
        _activePriority[newPriority] = newText;

        if (newPriority >= _currentPriority)
        {
            switch (newPriority)
            {
                case DisplayPriority.Playing:
                    CRManager.root.Restart(AnimatePlayingText(newText), "BrTextAnim", this);
                    break;
                case DisplayPriority.Finder:
                    if (_currentPriority != newPriority)
                    {
                        CRManager.root.Restart(AnimateText(newText), "BrTextAnim", this);
                    }
                    else
                    {
                        GameInfoText.text = newText;
                    }
                    break;
                default:
                    CRManager.root.Restart(AnimateText(newText), "BrTextAnim", this);
                    break;
            }

            _currentPriority = newPriority;
        }
    }
    private IEnumerator AnimateText(string newText)
    {
        if (GameInfoText.text.Contains("Playing:"))
        {
            yield return new WaitForSeconds(0.25f);
        }
        int matchIndex = 0;
        string oldText = GameInfoText.text;

        while (matchIndex < oldText.Length && matchIndex < newText.Length && oldText[matchIndex] == newText[matchIndex])
        {
            matchIndex++;
        }

        for (int i = oldText.Length; i > matchIndex; i --)
        {
            GameInfoText.text = oldText.Substring(0, i - 1);

            if (i % 3 == 0)
                yield return new WaitForSeconds(0.01f);
        }

        for (int i = matchIndex; i <= newText.Length; i++)
        {
            GameInfoText.text = newText.Substring(0, i);
            yield return new WaitForSeconds(0.01f);

            if (!(oldText.Contains("Estimated") && newText.Contains("Estimated")) && i % 4 == 0 && GameManager.root.State != GameState.Shutdown)
                AudioManager.root.PlaySound(AudioEvent.playTextBeep, gameObject);
        }

    }
    private IEnumerator AnimatePlayingText(string newText)
    {
        var diff = DiffText(GameInfoText.text, newText);
        string display = diff.prefix;

        foreach (var token in diff.removeTokens)
        {
            display += $"<s>{token}</s>";
            GameInfoText.text = "Playing: \n" + display + diff.suffix;
            yield return new WaitForSeconds(0.025f);
            display = display.Replace($"<s>{token}</s>", "");
        }

        foreach (var token in diff.addTokens)
        {
            if (!string.IsNullOrEmpty(display) && !display.EndsWith(" & "))
                display += " & ";
            display += token;
            GameInfoText.text = "Playing: \n" + display + diff.suffix;
            yield return new WaitForSeconds(0.025f);
        }
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
    public void SetFinderText(string gemNote)
    {
        DisplayText($"Estimated Resonance: \n {gemNote}", DisplayPriority.Finder);
    }
    public void SetPlayingText(string newNote, bool addNote)
    {
        if (addNote)
        {
            _heldNotes.Add(newNote);
        }
        else
        {
            _heldNotes.Remove(newNote);
        }

        string textToSet = "Playing: \n";

        if (_heldNotes.Count > 0)
        {
            _heldNotes.Sort();

            if (_heldNotes.Count > 1)
            {
                for (int i = 0; i < _heldNotes.Count - 1; i++)
                {
                    textToSet += $"{_heldNotes[i]} & ";
                }
            }

            textToSet += _heldNotes[_heldNotes.Count - 1];
        }
        else
        {
            LowerTextPriority(DisplayPriority.Playing);
            return;
        }

        DisplayText(textToSet, DisplayPriority.Playing);
    }
    public void SetSyncText(string startNote)
    {
        DisplayText($"Sync Plate detected \n Play {startNote} to connect", DisplayPriority.Sync);
    }
    [AllowAllAboveState(GameState.Settings), DissallowedStates(GameState.Intro)]
    public void OpenSettings(float newPauseAmount)
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
    private (List<string> removeTokens, List<string> addTokens, string prefix, string suffix) DiffText(string oldText, string newText)
    {
        string prefix = "Playing: \n";
        string oldBody = oldText.StartsWith(prefix) ? oldText.Substring(prefix.Length) : oldText;
        string newBody = newText.StartsWith(prefix) ? newText.Substring(prefix.Length) : newText;

        string[] oldTokens = oldBody.Split(new[] { " & " }, StringSplitOptions.None);
        string[] newTokens = newBody.Split(new[] { " & " }, StringSplitOptions.None);

        int startMatch = 0;
        while (startMatch < oldTokens.Length && startMatch < newTokens.Length && oldTokens[startMatch] == newTokens[startMatch])
        {
            startMatch++;
        }

        int endMatch = 0;
        while (endMatch + startMatch < oldTokens.Length && endMatch + startMatch < newTokens.Length && oldTokens[oldTokens.Length - 1 - endMatch] == newTokens[newTokens.Length - 1 - endMatch])
        {
            endMatch++;
        }

        List<string> removeTokens = new List<string>();
        for (int i = startMatch; i < oldTokens.Length - endMatch; i++)
        {
            removeTokens.Add(oldTokens[i]);
        }

        List<string> addTokens = new List<string>();
        for (int i = startMatch; i < newTokens.Length - endMatch; i++)
        {
            addTokens.Add(newTokens[i]);
        }

        string matchedPrefix = string.Join(" & ", newTokens[..startMatch]);
        if (matchedPrefix != "") matchedPrefix += " & ";

        string matchedSuffix = string.Join(" & ", newTokens[(newTokens.Length - endMatch)..]);
        if (matchedSuffix != "") matchedSuffix = " & " + matchedSuffix;

        return (removeTokens, addTokens, matchedPrefix, matchedSuffix);
    }
}
