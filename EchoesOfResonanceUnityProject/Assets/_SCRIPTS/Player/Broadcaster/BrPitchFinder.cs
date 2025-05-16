using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class BrPitchFinder : Broadcaster
{
    public static Action CheckFinder;
    public int laserResolution;
    [SerializeField] private float gemFinderSpeedScale, gemFinderMinSpeed;
    [SerializeField] private float laserWobbleStrength, laserWobbleSpeed;
    [SerializeField] private LineRenderer gemLaser;
    [SerializeField] private ParticleSystem gemLaserParticle;
    private float smoothedInput;
    private PzNote gemNote = new PzNote(13);
    public Transform gemTarget;
    public override void Awake()
    {
        RegisterActiveBroadcaster(this);
        gemLaser.positionCount = laserResolution;

        OnHeldNotesEmptied += UpdateGemFinder;
        OnBatteryEmpty += DisableFinder;
        CheckFinder += UpdateGemFinder;
    }
    public override void OnNoteOn(int newNote, int velocity)
    {
        DisableFinder();
    }
    public override void ModChange(float newModInput)
    {
        UpdateGemFinder();
    }
    public override void OnPuzzleExit()
    {
        DisableFinder();
    }
    public void UpdateGemFinder()
    {
        if (modInput > 0.2f && activePuzzle.solutions.Length > activePuzzle.solved) EnableFinder();
        else DisableFinder();
    }

    void EnableFinder()
    {
        var target = activePlate.gems[activePuzzle.solved].transform;

        if (gemTarget != target)
        {
            gemTarget = target;

            gemLaser.enabled = true;
            gemLaserParticle.Play();

            gemNote = activePuzzle.solutions[activePuzzle.solved].note;

            CRManager.Begin(FindGemRoutine(), "FindGem", this);
            CRManager.Begin(LaserRoutine(), "DisplayLaser", this);
        }
    }
    public void DisableFinder()
    {
        modInput = 0;

        Vector3[] positions = Enumerable.Repeat(Vector3.zero, laserResolution).ToArray();
        gemLaser.SetPositions(positions);
        gemLaser.enabled = false;
        gemLaserParticle.Stop();

        gemTarget = null;

        CRManager.Stop("FindGem", this);
        CRManager.Stop("DisplayLaser", this);
    }
    IEnumerator FindGemRoutine()
    {
        while (modInput > 0.2)
        {
            finderEstimate = new PzNote(Mathf.Round(UnityEngine.Random.Range(
                gemNote.Pitch - 1.5f * (4 - finderLevel),
                gemNote.Pitch + 1.5f * (4 - finderLevel)
            )));

            AudioManager.root.SetRTPC(AudioRTPC.finder_Pitch, finderEstimate.Pitch);

            yield return new WaitForSeconds(gemFinderMinSpeed + ((4 - finderLevel) * gemFinderSpeedScale));
        }
    }

    IEnumerator LaserRoutine()
    {
        float timeOffset = UnityEngine.Random.Range(0f, 1000f);

        while (modInput > 0.2f)
        {
            smoothedInput = Mathf.Lerp(smoothedInput, Mathf.InverseLerp(0.2f, 1f, modInput), Time.deltaTime * 2.5f);
            
            float noiseScale = Mathf.Lerp(laserWobbleSpeed, laserWobbleSpeed / 2f, smoothedInput);
            float wobbleStrength = Mathf.Lerp(laserWobbleStrength, laserWobbleStrength / 2f, smoothedInput);
            float t = Time.time;

            for (int i = 0; i < laserResolution; i++)
            {
                float progress = (float)i / laserResolution;
                Vector3 basePos = Vector3.Lerp(gemLaser.transform.position, gemTarget.position, progress);

                if (i > 0)
                {
                    float noiseX = Mathf.PerlinNoise(t * noiseScale + i * 0.1f + timeOffset, 0f);
                    float noiseY = Mathf.PerlinNoise(t * noiseScale + i * 0.1f + timeOffset, 100f);
                    float noiseZ = Mathf.PerlinNoise(t * noiseScale + i * 0.1f + timeOffset, 200f);

                    Vector3 noiseVec = new Vector3(noiseX - 0.5f, noiseY - 0.5f, noiseZ - 0.5f) * wobbleStrength;

                    gemLaser.SetPosition(i, basePos + noiseVec);
                }
                else
                {
                    gemLaser.SetPosition(i, basePos);
                }
            }

            yield return null;
        }
    }
    public override void OnDestroy()
    {
        OnHeldNotesEmptied -= UpdateGemFinder;
        OnBatteryEmpty -= DisableFinder;
        CheckFinder -= UpdateGemFinder;
    }
}