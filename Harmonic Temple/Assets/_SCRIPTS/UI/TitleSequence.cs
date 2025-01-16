using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class TitleSequence : MonoBehaviour, IInputScript
{
    [SerializeField] private float textSpeed, buttonSpeed;
         [TextArea(5,5)]
    [SerializeField] private string openingCreditText, titleScreenText;
    [SerializeField] private Animator broadcasterAnim;
    [SerializeField] private GameObject broadcasterInfo;
    [SerializeField] private AK.Wwise.Event beep;
    private TextMeshProUGUI titleCreditTextManager, titleScreenTextManager;
    private Image beginButton;
    private Coroutine titleRoutine;
    void Awake()
    {
        titleCreditTextManager = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        titleScreenTextManager = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        beginButton = GetComponentInChildren<Image>();

        titleCreditTextManager.text = "";
        titleScreenTextManager.text = "";
        beginButton.enabled = false;
        broadcasterInfo.SetActive(false);
    }

    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.KeyDown, ToGameplay);
    }

    public IEnumerator PlayTitleCredits()
    {
        string displayedText = "";
        yield return new WaitForSeconds(0.75f);

        while (displayedText.Length < openingCreditText.Length)
        {
            if (openingCreditText[displayedText.Length].ToString() != " ")
            {
                yield return new WaitForSeconds(textSpeed);
                beep.Post(broadcasterAnim.gameObject);
            }


            displayedText += openingCreditText[displayedText.Length];
            titleCreditTextManager.text = displayedText;
        }

        yield return new WaitForSeconds(1.5f);

        titleCreditTextManager.text = "";
        titleRoutine = StartCoroutine(PlayTitleScreen());
    }
    IEnumerator PlayTitleScreen()
    {
        titleScreenTextManager.text = titleScreenText;

        while(GameManager.root.currentState == GameState.Title)
        {
            beginButton.enabled = !beginButton.enabled;
            yield return new WaitForSeconds(beginButton.enabled? buttonSpeed * 1.5f : buttonSpeed);
        }
    }
    void ToGameplay(float newNote)
    {
        if (GameManager.root.currentState == GameState.Title && titleRoutine != null)
        {
            broadcasterAnim.SetTrigger("toGameplay");
            GameManager.root.currentState = GameState.InPuzzle;
            StopAllCoroutines();
            broadcasterInfo.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
