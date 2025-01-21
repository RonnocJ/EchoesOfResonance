using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class MenuItem
{
    public Image itemImage;
    public Action<float> method;
    public MenuItem(Image newImage, Action<float> newMethod)
    {
        itemImage = newImage;
        method = newMethod;
    }
}
public class SettingsMenu : MonoBehaviour, IInputScript
{
    [Header("Buttons")]
    [SerializeField] private Image sfxImage;
    [SerializeField] private Image musicImage, hintImage, keyboardImage, quitImage;
    [Header("Sliders")]
    [SerializeField] private Image sfxSlider;
    [SerializeField] private Image musicSlider;
    [SerializeField] private AK.Wwise.RTPC volumeSFX, volumeMusic;
    [SerializeField] private TextMeshProUGUI sfxText, musicText;
    [Header("Hints")]
    [SerializeField] private GameObject[] hints;
    [Header("Keyboard Layout")]
    [SerializeField] private GameObject keyboardLayout;
    [SerializeField] private AK.Wwise.Event pauseSounds, resumeSounds;

    [SerializeField] private bool moved;
    [SerializeField] private int itemIndex;
    private GameState oldState;
    private List<MenuItem> allItems;
    void Awake()
    {
        allItems = new List<MenuItem> {
            new MenuItem(sfxImage, VolumeSFXSlider),
            new MenuItem(musicImage, VolumeMusicSlider),
            new MenuItem(hintImage, ShowHints),
            new MenuItem(keyboardImage, ShowKeyboard),
            new MenuItem(quitImage, QuitGameButton)
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
            allItems[itemIndex] = allItems[itemIndex];

            allItems[itemIndex + 1].itemImage.color = Color.black;
            allItems[itemIndex].itemImage.color = Color.white;

            if (allItems[itemIndex + 1].itemImage == hintImage)
            {
                UIFade.root.SetAlpha(0, hints.ToList());
            }
            else if (allItems[itemIndex + 1].itemImage == keyboardImage)
            {
                UIFade.root.SetAlpha(0, new List<GameObject> { keyboardLayout });
            }
        }

        if (itemIndex < allItems.Count - 1 && input == 1f && !moved)
        {
            moved = true;

            itemIndex++;
            allItems[itemIndex] = allItems[itemIndex];

            allItems[itemIndex - 1].itemImage.color = Color.black;
            allItems[itemIndex].itemImage.color = Color.white;
            Debug.Log("Updated Colors!");
            if (allItems[itemIndex - 1].itemImage == hintImage)
            {
                UIFade.root.SetAlpha(0, hints.ToList());
            }
            else if (allItems[itemIndex - 1].itemImage == keyboardImage)
            {
                UIFade.root.SetAlpha(0, new List<GameObject> { keyboardLayout });
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
        sfxText.text = Mathf.Round(input * 100).ToString();
        volumeSFX.SetValue(gameObject, input * 100f);
    }
    void VolumeMusicSlider(float input)
    {
        musicSlider.fillAmount = input;
        musicText.text = Mathf.Round(input * 100).ToString();
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