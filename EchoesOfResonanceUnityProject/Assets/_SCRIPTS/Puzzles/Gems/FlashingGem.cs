using UnityEngine;

public class FlashingGem : Gem
{
    public float sequencePos;
    public float sequenceDelay;
    public ParticleSystem gemFlash;
    /*public void LinkToMusic()
    {
        MusicManager.root.currentSong.AddBeatListener(1, FlashGem);
    }

    void FlashGem()
    {
        if(MusicManager.root.currentSong.grid % sequenceDelay == sequencePos)
        {
            gemFlash.Play();
            MusicManager.root.currentSong.RemoveBeatListener(1, FlashGem);
        }
    }*/
}
