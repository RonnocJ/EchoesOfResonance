using System.Collections;
using UnityEngine;

public class TorchManager : BasicPuzzle
{
    [SerializeField] private GlobalTorchData globalTorchData;
    [SerializeField] private int earlyCompletion = -1;
    [SerializeField] private float torchTimer = -1;
    private ParticleSystem flameParticle, glowParticle;
    private bool alreadyPlayed;
    private Light torchLight;

    public override void Awake()
    {
        base.Awake();

        flameParticle = transform.GetChild(0).GetComponent<ParticleSystem>();
        glowParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
        torchLight = transform.GetChild(2).GetComponent<Light>();
        torchLight.enabled = false;

        alreadyPlayed = false;

        if (earlyCompletion > -1)
            attachedData.OnValueChanged += ActivateEarly;
    }
    public void ActivateEarly(int solveCheck)
    {
        if (solveCheck == earlyCompletion)
        {
            flameParticle.Play();
            glowParticle.Play();
            torchLight.enabled = true;
            globalTorchData.torchLightUp.Post(gameObject);
            alreadyPlayed = true;

            if (torchTimer > -1)
            {
                CRManager.root.Begin(ExtinguishTorch(false, torchTimer), $"Extinguish{gameObject.name}", this);
            }
        }
    }
    public void ExtinguishMethod(float delay)
    {
        CRManager.root.Begin(ExtinguishTorch(false, delay), $"Extinguish{gameObject.name}", this);
    }
    public IEnumerator ExtinguishTorch(bool overrideSolution, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PuzzleManager.root.currentPuzzle.solved < PuzzleManager.root.currentPuzzle.solutions.Length && torchLight.enabled)
        {
            flameParticle.Stop();
            glowParticle.Stop();
            torchLight.enabled = false;
            globalTorchData.torchExtinguish.Post(gameObject);
            PuzzleManager.root.ResetPuzzle();
        }
        else if(overrideSolution && torchLight.enabled)
        {
            flameParticle.Stop();
            glowParticle.Stop();
            torchLight.enabled = false;
            globalTorchData.torchExtinguish.Post(gameObject);
        }
    }
    public override void FinishedPuzzle()
    {
        base.FinishedPuzzle();
        if (!alreadyPlayed)
        {
            flameParticle.Play();
            glowParticle.Play();
            torchLight.enabled = true;
            globalTorchData.torchLightUp.Post(gameObject);
        }
    }
}
