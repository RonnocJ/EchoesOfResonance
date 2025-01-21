using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
public class TitleSequence : MonoBehaviour, IInputScript
{
    [SerializeField] private float textSpeed, buttonSpeed;
    [TextArea(5, 5)]
    [SerializeField] private string openingCreditText, titleScreenText;
    [SerializeField] private Animator broadcasterAnim;
    [SerializeField] private GameObject broadcasterInfo;
    [SerializeField] private PuzzleData finalPuzzle;
    [SerializeField] GameHints hintManager;
    [SerializeField] private TempleManager templeManager;
    [SerializeField] private AK.Wwise.Event beep, bloop, plunk, playMainMusic, stopMainMusic;
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
        templeManager.StartAmbiences();
        string displayedText = "";
        yield return new WaitForSeconds(1.25f);

        while (displayedText.Length < openingCreditText.Length)
        {
            displayedText += openingCreditText[displayedText.Length];
            titleCreditTextManager.text = displayedText;

            if (openingCreditText[displayedText.Length - 1].ToString() != " ")
            {
                yield return new WaitForSeconds(textSpeed);
                beep.Post(broadcasterAnim.gameObject);
            }
            else
            {
                yield return new WaitForSeconds(textSpeed * 1.5f);
            }
        }

        yield return new WaitForSeconds(2f);

        plunk.Post(gameObject);
        titleCreditTextManager.text = "";
        titleRoutine = StartCoroutine(PlayTitleScreen());
    }
    IEnumerator PlayTitleScreen()
    {
        yield return new WaitForSeconds(1f);
        titleScreenTextManager.text = titleScreenText;
        bloop.Post(gameObject);
        
        while (GameManager.root.currentState == GameState.Title)
        {
            beginButton.enabled = !beginButton.enabled;
            yield return new WaitForSeconds(beginButton.enabled ? buttonSpeed * 1.5f : buttonSpeed);
        }
    }
    void ToGameplay(float newNote)
    {
        if (GameManager.root.currentState == GameState.Title && titleRoutine != null)
        {
            broadcasterAnim.SetTrigger("toGameplay");
            playMainMusic.Post(gameObject);

            StopAllCoroutines();

            hintManager.StartCoroutine(hintManager.ShowHints());

            broadcasterInfo.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void EndMusic()
    {
        stopMainMusic.Post(gameObject);
    }
}
