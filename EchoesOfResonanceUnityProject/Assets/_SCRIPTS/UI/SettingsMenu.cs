using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class SettingsMenuEntry
{
    public RectTransform itemTr;
    public Action<float> method;
    public SettingsMenuEntry(RectTransform newTr, Action<float> newMethod)
    {
        itemTr = newTr;
        method = newMethod;
    }
}
public class SettingsMenu : MonoBehaviour, IInputScript
{
    [Header("Highlighter")]
    [SerializeField] private RectTransform highlighter;
    [Header("Buttons")]
    [SerializeField] private RectTransform sfxText;
    [SerializeField] private RectTransform musicText, hintText, keyboardText, quitText;
    [Header("Sliders")]
    [SerializeField] private Image sfxSlider;
    [SerializeField] private Image musicSlider;
    [SerializeField] private AK.Wwise.RTPC volumeSFX, volumeMusic;
    [SerializeField] private TextMeshProUGUI sfxLevelText, musicLevelText;
    [Header("Hints")]
    [SerializeField] private GameObject[] hints;
    [Header("Keyboard Layout")]
    [SerializeField] private GameObject keyboardLayout;
    [SerializeField] private AK.Wwise.Event pauseSounds, resumeSounds;

    [SerializeField] private bool moved;
    [SerializeField] private int itemIndex;
    private GameState oldState;
    private List<SettingsMenuEntry> allItems;
    void Awake()
    {
        allItems = new List<SettingsMenuEntry> {
            new SettingsMenuEntry(sfxText, VolumeSFXSlider),
            new SettingsMenuEntry(musicText, VolumeMusicSlider),
            new SettingsMenuEntry(hintText, ShowHints),
            new SettingsMenuEntry(keyboardText, ShowKeyboard),
            new SettingsMenuEntry(quitText, QuitGameButton)
            };
        itemIndex = 0;

        
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.Settings, OpenOrCloseMenu);
        InputManager.root.AddListener<float>(ActionTypes.PitchbendChange, ScrollItems);
        InputManager.root.AddListener<float>(ActionTypes.ModwheelChange, AdjustValue);
    }

    [AllowedStates(GameState.Title, GameState.Settings, GameState.InPuzzle, GameState.Roaming, GameState.Shutdown)]
    void OpenOrCloseMenu(float settingsInput)
    {
        if (settingsInput > 0.5f && GameManager.root.currentState != GameState.Settings)
        {
            oldState = GameManager.root.currentState;
            GameManager.root.currentState = GameState.Settings;
            pauseSounds.Post(gameObject);

            Time.timeScale = 0;

            transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (settingsInput == 0 && GameManager.root.currentState == GameState.Settings)
        {
            GameManager.root.currentState = oldState;
            resumeSounds.Post(gameObject);

            Time.timeScale = 1;

            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    [AllowedStates(GameState.Settings)]
    void ScrollItems(float pitchInput)
    {
        float input = 0;

        switch (pitchInput)
        {
            case > 0.5f:
                input = -1;
                break;
            case < -0.5f:
                input = 1;
                break;
            case < 0.1f and > -0.1f:
                input = 0;
                moved = false;
                break;
        }
        if (itemIndex > 0 && input == -1f && !moved)
        {
            moved = true;
            itemIndex--;

            highlighter.position = allItems[itemIndex].itemTr.position;

            if (allItems[itemIndex + 1].itemTr == hintText)
            {
                ShowHints(0);
            }
            else if (allItems[itemIndex + 1].itemTr == keyboardText)
            {
                ShowKeyboard(0);
            }
        }

        if (itemIndex < allItems.Count - 1 && input == 1f && !moved)
        {
            moved = true;
            itemIndex++;

            highlighter.position = allItems[itemIndex - 1].itemTr.position;

            if (allItems[itemIndex].itemTr == hintText)
            {
                ShowHints(0);
            }
            else if (allItems[itemIndex].itemTr == keyboardText)
            {
                ShowKeyboard(0);
            }
        }
    }
    [AllowedStates(GameState.Settings)]
    void AdjustValue(float modInput)
    {
        allItems[itemIndex].method.Invoke(Mathf.Clamp((modInput * 1.25f) - 0.2f, 0f, 1f));
    }
    void VolumeSFXSlider(float input)
    {
        sfxSlider.fillAmount = input;
        sfxLevelText.text = Mathf.Round(input * 100).ToString();
        volumeSFX.SetValue(gameObject, input * 100f);
    }
    void VolumeMusicSlider(float input)
    {
        musicSlider.fillAmount = input;
        musicLevelText.text = Mathf.Round(input * 100).ToString();
        volumeMusic.SetValue(gameObject, input * 100f);
    }
    void ShowHints(float input)
    {
        UIFade.root.SetAlpha(input, hints.ToList());
    }
    void ShowKeyboard(float input)
    {
        UIFade.root.SetAlpha(input, new List<GameObject> { keyboardLayout });
    }
    void QuitGameButton(float input)
    {
        if (input == 1)
        {
            Application.Quit();
        }
    }
}