using UnityEngine;

public class TorchManager : BasicPuzzle
{
    [SerializeField] private GlobalTorchData globalTorchData;
    private ParticleSystem flameParticle, glowParticle;
    private Light torchLight;

    void Awake()
    {
        flameParticle = transform.GetChild(0).GetComponent<ParticleSystem>();
        glowParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
        torchLight = transform.GetChild(2).GetComponent<Light>();
        torchLight.enabled = false;
    }

    public override void FinishedPuzzle()
    {
        base.FinishedPuzzle();
        flameParticle.Play();
        glowParticle.Play();
        torchLight.enabled = true;
    }
}
