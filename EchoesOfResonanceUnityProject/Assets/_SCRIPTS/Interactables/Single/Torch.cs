using System.Collections;
using UnityEngine;

public class Torch : BasicInteractable
{
    [SerializeField] private bool playOnAwake;
    [SerializeField] private float torchTimer;
    private ParticleSystem flameParticle, glowParticle;
    private Light torchLight;
    void Start()
    {
        flameParticle = transform.GetChild(0).GetComponent<ParticleSystem>();
        glowParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
        torchLight = transform.GetChild(2).GetComponent<Light>();

        if (playOnAwake)
        {    
            base.ActivateObject();
            
            flameParticle.Play();
            glowParticle.Play();
            torchLight.enabled = true;

            AudioManager.root.PlaySound(AudioEvent.playTorchLitLoop, gameObject, 1);
        }
    }
    public override void ActivateObject()
    {
        base.ActivateObject();

        flameParticle.Play();
        glowParticle.Play();
        torchLight.enabled = true;
       //AudioManager.root.PlaySound(AudioEvent.playTorchIgnite, gameObject);
    }
    public override void ResetObject()
    {
        base.ResetObject();

        if (torchLight.enabled)
        {
            flameParticle.Stop();
            glowParticle.Stop();
            torchLight.enabled = false;
            //AudioManager.root.PlaySound(AudioEvent.playTorchExtinguish, gameObject);
        }
    }
}
