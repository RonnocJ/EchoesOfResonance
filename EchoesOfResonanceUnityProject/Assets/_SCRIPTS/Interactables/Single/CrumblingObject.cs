using UnityEngine;

public class CrumblingObject : BasicInteractable
{
    public override void ActivateObject()
    {
        base.ActivateObject();
        AudioManager.root.PlaySound(AudioEvent.playStoneCrumble, gameObject);
        Destroy(gameObject);
    }
}