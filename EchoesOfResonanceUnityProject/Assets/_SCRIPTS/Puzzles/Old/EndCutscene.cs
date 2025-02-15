using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCutscene : MonoBehaviour
{
    /*[SerializeField] private PuzzleData finalPuzzle;
    [SerializeField] private AK.Wwise.Event endRiser;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TempleManager temple;
    private Light blindingLight;
    private Camera cam;
    private Transform broadcasterTr;
    void Awake()
    {
        blindingLight = GetComponent<Light>();
        cam = Camera.main;
        broadcasterTr = cam.transform.GetChild(0);

        finalPuzzle.OnPuzzleCompleted += () => CRManager.root.Begin(FinalCutscene(), "FinalCutscene", this);
    }

    IEnumerator FinalCutscene()
    {
        endRiser.Post(gameObject);

        float elapsed = 0f;

        while (elapsed < 10f)
        {
            if (elapsed > 0.5f && GameManager.root.currentState != GameState.Final)
            {

                Broadcaster.root.AllNotesOff();
                Broadcaster.root.notesHeld = 0;
                Broadcaster.root.RemoveNote(0);
                Broadcaster.root.AdjustModValue(0);

                GameManager.root.currentState = GameState.Final;
            }
            else if (elapsed > 8f && !CRManager.root.IsRunning("FadeInEndScreen"))
            {

                CRManager.root.Begin(UIFade.root.FadeItems(2.5f, 0f, false, new List<GameObject> { endScreen }), "FadeInEndScreen", this);
            }

            cam.transform.parent.localPosition = Vector3.Lerp(cam.transform.parent.localPosition, new Vector3(-12.5f, 33, 70), Time.deltaTime * 4f);
            cam.transform.parent.localEulerAngles = Vector3.Lerp(cam.transform.parent.localEulerAngles, new Vector3(0, 160, 0), Time.deltaTime * 4f);
            broadcasterTr.localPosition = Vector3.Lerp(broadcasterTr.localPosition, new Vector3(broadcasterTr.localPosition.x, -2f, broadcasterTr.localPosition.z), Time.deltaTime * 4f);

            cam.transform.localPosition = Random.insideUnitSphere * 0.1f * elapsed;
            blindingLight.intensity = Mathf.Pow(Mathf.Pow(elapsed, 3), 3);
            elapsed += Time.deltaTime;
            yield return null;
        }

        temple.StopAmbiences();

        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(2.5f);
            CRManager.root.Begin(UIFade.root.FadeItems(0.5f, 0f, false, new List<GameObject> { endScreen.transform.GetChild(i).gameObject }), $"FadeInEndCredit{i}", this);
        }

        yield return new WaitForSeconds(4f);
        Application.Quit();
    }*/
}
