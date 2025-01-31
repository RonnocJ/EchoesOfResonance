using System.Collections;
using UnityEngine;

public class TorchManager : BasicPuzzle
{
    [SerializeField] private int earlyCompletion = -1;
    [SerializeField] private float torchTimer = -1;
    public ParticleSystem flameParticle, glowParticle;
    public Light torchLight;
    private bool alreadyPlayed;

    void Awake()
    {
        alreadyPlayed = false;

        if (earlyCompletion > -1)
            linkedData.OnSolvedChanged += ActivateEarly;
    }
    public void ActivateEarly(int solveCheck)
    {
        if (solveCheck == earlyCompletion)
        {
            flameParticle.Play();
            glowParticle.Play();
            torchLight.enabled = true;
            DH.Get<GlobalTorchData>().torchLightUp.Post(gameObject);
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

        if (torchLight.enabled && (overrideSolution || linkedData.solved < linkedData.solutions.Length))
        {
            flameParticle.Stop();
            glowParticle.Stop();
            torchLight.enabled = false;
            DH.Get<GlobalTorchData>().torchExtinguish.Post(gameObject);
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
            DH.Get<GlobalTorchData>().torchLightUp.Post(gameObject);
        }
    }
}
