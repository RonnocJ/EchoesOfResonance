using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigureKeyboard : Singleton<ConfigureKeyboard>, IInputScript
{
    [HideInInspector]
    public float middleKey, settingsCC, tempMiddleKey, tempSettingsCC;
    public TitleSequence titleSequence;
    [SerializeField] private GameObject keyboardConfig, useKeyboard, useKeyboardText, keyboardConfirm, keyboardConfirmText, settingsConfig, settingsConfirm, settingsConfirmText;
    private bool inSettingsMenu;


    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyUp, SetMiddleC);
        InputManager.root.AddListener<float>(ActionTypes.Settings, SetSettingsCC);
    }
    void Start()
    {
        GameManager.root.currentState = GameState.Config;
        inSettingsMenu = false;
        middleKey = -1;
        tempMiddleKey = -1;
        settingsCC = -1;
        tempSettingsCC = -1;

        UIFade.root.SetAlpha(1, new List<GameObject> { keyboardConfig, useKeyboard, useKeyboardText });
        UIFade.root.SetAlpha(0, new List<GameObject> { keyboardConfirm, keyboardConfirmText, settingsConfig, settingsConfirm, settingsConfirmText });
    }
    [AllowedStates(GameState.Config)]
    void SetMiddleC(float newMiddle)
    {
        if (middleKey == -1)
        {
            if (newMiddle == tempMiddleKey)
            {
                middleKey = newMiddle;
                ToSettingsMenu();
            }
            else if (tempMiddleKey != -1)
            {
                CRManager.root.Begin(UIFade.root.FadeItems(0.25f * DH.Get<TestOverrides>().uiSpeed, 0, true, new List<GameObject> { keyboardConfirm, keyboardConfirmText }), "FadeOutKeyboardConfirm", this);
                tempMiddleKey = -1;
            }
            else if (tempMiddleKey == -1)
            {
                CRManager.root.Begin(UIFade.root.FadeItems(0.25f * DH.Get<TestOverrides>().uiSpeed, 0, false, new List<GameObject> { keyboardConfirm, keyboardConfirmText }), "FadeInKeyboardConfirm", this);
                tempMiddleKey = newMiddle;
            }
        }
    }
    public void ToSettingsMenu()
    {
        if (InputManager.root.usingMidiKeyboard)
        {
            CRManager.root.Begin(UIFade.root.FadeItems(1f * DH.Get<TestOverrides>().uiSpeed, 0, true, new List<GameObject> { keyboardConfig, useKeyboard, useKeyboardText, keyboardConfirm, keyboardConfirmText }), "FadeOutKeyboard", this);
        }
        else
        {
            CRManager.root.Begin(UIFade.root.FadeItems(1f * DH.Get<TestOverrides>().uiSpeed, 0, true, new List<GameObject> { keyboardConfig, useKeyboard, useKeyboardText }), "FadeOutKeyboard", this);
        }

        CRManager.root.Begin(UIFade.root.FadeItems(0.5f * DH.Get<TestOverrides>().uiSpeed, 1.5f * DH.Get<TestOverrides>().uiSpeed, false, new List<GameObject> { settingsConfig }), "FadeInSettings", this);

        inSettingsMenu = true;
    }
    [AllowedStates(GameState.Config)]
    void SetSettingsCC(float newKnob)
    {
        if (inSettingsMenu)
        {
            if (InputManager.root.usingEscKey)
            {
                InputManager.root.settingsToggleState = false;
                ToTitle();
            }
            else if (newKnob == 0 && tempSettingsCC != -1)
            {
                ToTitle();
            }
            else if (newKnob > 0 && tempSettingsCC == -1)
            {
                CRManager.root.Begin(UIFade.root.FadeItems(0.5f * DH.Get<TestOverrides>().uiSpeed, 0, false, new List<GameObject> { settingsConfirm, settingsConfirmText }), "FadeInSettingsConfirm", this);
            }
        }
    }
    public void ToTitle()
    {
        CRManager.root.Begin(UIFade.root.FadeItems(1f * DH.Get<TestOverrides>().uiSpeed, 0, true, new List<GameObject> { gameObject, settingsConfig, settingsConfirm, settingsConfirmText }), "FadeOutSettings", this);
        
        if (!DH.Get<TestOverrides>().skipIntro)
            CRManager.root.Begin(titleSequence.PlayTitleCredits(), "PlayTitleCredits", titleSequence);
        else
        {
            titleSequence.ToGameplay(13);
        }
    }
}
