using System.Collections;
using UnityEngine;

public class Torch : BasicInteractable
{
    [SerializeField] private float torchTimer;
    public ParticleSystem flameParticle, glowParticle;
    public Light torchLight;

    public override void ActivateObject()
    {
        base.ActivateObject();

        flameParticle.Play();
        glowParticle.Play();
        torchLight.enabled = true;
       AudioManager.root.PlaySound(AudioEvent.playTorchIgnite, gameObject);
    }
    public override void ResetObject()
    {
        base.ResetObject();

        if (torchLight.enabled)
        {
            flameParticle.Stop();
            glowParticle.Stop();
            torchLight.enabled = false;
            AudioManager.root.PlaySound(AudioEvent.playTorchExtinguish, gameObject);
        }
    }
}
