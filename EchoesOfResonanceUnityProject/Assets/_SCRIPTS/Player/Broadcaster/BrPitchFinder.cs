using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BrPitchFinder : Singleton<BrPitchFinder>, IInputScript
{
    public int laserResolution;
    [SerializeField] private float gemFinderSpeedScale, gemFinderMinSpeed;
    [SerializeField] private LineRenderer gemLaser;
    [SerializeField] private ParticleSystem gemLaserParticle;
    private float lastNotePlayed, modInput, gemNoteNumber;
    private string gemNoteName;
    public Transform gemTarget;
    private List<Gem> currentGems;
    void Start()
    {
        lastNotePlayed = 13;
        gemLaser.positionCount = laserResolution;

        BrBattery.root.OnBatteryEmpty += DisableFinder;
    }
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.ModwheelChange, AdjustModValue);
        InputManager.root.AddListener<float>(ActionTypes.PitchbendChange, UpdateGemFinder);
    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming)]
    public void AdjustModValue(float modWheelAmount)
    {
        if (modWheelAmount > 0.2f && GameManager.root.currentState == GameState.InPuzzle)
        {
            modInput = modWheelAmount;

            UpdateGemFinder(0);
        }
        else
        {
            modInput = 0;
            DisableFinder();
        }
    }

    [AllowedStates(GameState.InPuzzle)]
    void UpdateGemFinder(float newPitch)
    {
        if (modInput > 0.2f)
        {
            if ((gemTarget == null || newPitch != 0f) && currentGems.Count > 0)
            {
                gemTarget = FindClosestGem(BrBattery.root.cam.transform.position, currentGems);

                gemLaser.enabled = true;
                gemLaserParticle.Play();

                if (gemNoteName != GameManager.root.currentPuzzle.solutions[gemTarget.GetSiblingIndex()].noteName)
                {
                    gemNoteName = GameManager.root.currentPuzzle.solutions[gemTarget.GetSiblingIndex()].noteName;
                    gemNoteNumber = PuzzleUtilities.root.GetNoteNumber(gemNoteName);

                    AudioManager.root.PlaySound(AudioEvent.playBroadcasterFinder, gameObject);

                    CRManager.root.Begin(FindGemRoutine(), "FindGem", this);
                    CRManager.root.Begin(BrBattery.root.DrainBatteryRoutine(modInput * 0.5f), "DrainBatteryFinder", this);
                }
                else if (BrBattery.root.notesHeld > 0 && BrBattery.root.noteInfoText.text != PuzzleUtilities.root.GetNoteName(lastNotePlayed))
                {
                    DisableFinder();
                }
            }
        }
        else if (gemTarget != null)
        {
            DisableFinder();
        }
    }
    Transform FindClosestGem(Vector3 position, List<Gem> gems)
    {
        return gems
            .OrderBy(gem => Vector3.Angle(gem.transform.position - position, BrBattery.root.cam.transform.forward))
            .FirstOrDefault()?.transform;
    }
    void DisableFinder()
    {
        BrBattery.root.noteInfoText.text = (BrBattery.root.notesHeld > 0) ? PuzzleUtilities.root.GetNoteName(lastNotePlayed) : "";

        Vector3[] positions = Enumerable.Repeat(Vector3.zero, laserResolution).ToArray();
        gemLaser.SetPositions(positions);
        gemLaser.enabled = false;
        gemLaserParticle.Stop();

        gemTarget = null;

        if(AudioManager.root.StopSound(AudioEvent.playBroadcasterFinder, gameObject)) Debug.Log("Stopped finder");

        CRManager.root.Stop("FindGem", this);
        CRManager.root.Stop("DrainBatteryFinder", this);

        gemNoteNumber = -1;
        gemNoteName = "";
    }
    IEnumerator FindGemRoutine()
    {
        while (modInput > 0.2f)
        {
            float finderLevel = Mathf.Round(((modInput * 1.25f) - 0.2f) / 0.25f);

            float fakeNoteNumber = Mathf.Round(Random.Range(
                gemNoteNumber - 1.5f * (4 - finderLevel),
                gemNoteNumber + 1.5f * (4 - finderLevel)
            ));

            fakeNoteNumber = Mathf.Clamp(fakeNoteNumber, 1, 25);
            
            BrBattery.root.noteInfoText.text = PuzzleUtilities.root.GetNoteName(fakeNoteNumber);
            AudioManager.root.SetRTPC(AudioRTPC.finder_Pitch, fakeNoteNumber);

            yield return new WaitForSeconds(gemFinderMinSpeed + ((4 - finderLevel) * gemFinderSpeedScale));
        }
    }
    void Update()
    {
        if (gemLaser.enabled)
        {
            for (int i = 0; i < laserResolution; i++)
            {
                Vector3 pos = Vector3.Lerp(gemLaser.transform.position, gemTarget.position, (float)i / laserResolution);
                gemLaser.SetPosition(i, (i > 0) ? (Random.insideUnitSphere * Mathf.Abs(1 - modInput) * 0.25f) + pos : pos);
            }

            if (GameManager.root.currentState == GameState.Roaming) AdjustModValue(0);
        }
    }

    public void SetGemList(List<Gem> gems)
    {
        currentGems = gems;
    }
}