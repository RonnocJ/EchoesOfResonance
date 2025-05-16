using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicTriggerZone : MonoBehaviour
{
    public MusicTracker musicTracker;
    public bool specialExecution;
    public AudioEvent musicEvent;
    public AudioState musicState;
    public GameObject[] beatListeners;
    private bool inTrigger, played;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            inTrigger = true;
            if (!specialExecution) ExecuteSounds();
        }
    }

    public void ExecuteSounds()
    {
        if (inTrigger && !played)
        {
            MusicManager.root.StopSong();
            MusicManager.root.PlaySong(musicTracker);
            
            if(musicState != AudioState.None)
                MusicManager.root.SetState(musicState);

            List<IBeatListener> listeners = new();

            foreach (var listener in beatListeners)
            {
                listeners.AddRange(listener.GetComponents<IBeatListener>());
            }

            listeners.ForEach(c => c.SubscribeToMusic());

            played = true;
        }
    }
}
