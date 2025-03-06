using System.Collections.Generic;
using UnityEngine;
public enum BeatFrequency
{
    Grid,
    Beat,
    Bar,
    Loop
}
public class MusicManager : Singleton<MusicManager>
{
    public MusicTracker currentSong;
    private readonly Dictionary<AudioEvent, AudioEvent> musicMetronomeRef = new()
    {
        {AudioEvent.startMusic01, AudioEvent.startMetronome01},
        {AudioEvent.playGemTestTrack, AudioEvent.playTestMetronome} 
    };
    private Dictionary<AudioEvent, uint> playingMusicIds = new();
    public void PlaySong(AudioEvent songType, MusicTracker newSong)
    {
        if (!playingMusicIds.ContainsKey(songType) && musicMetronomeRef.TryGetValue(songType, out AudioEvent metronome) && songType != AudioEvent.None)
        {
            currentSong = newSong;
            playingMusicIds[songType] = AkUnitySoundEngine.PostEvent(songType.ToString(), gameObject);
            playingMusicIds[metronome] = AkUnitySoundEngine.PostEvent(metronome.ToString(), gameObject, (uint)(AkCallbackType.AK_MusicSyncAll | AkCallbackType.AK_EnableGetMusicPlayPosition), newSong.MusicCallbackFunction, null);            
        }
    }

    public void StopSong(AudioEvent songType)
    {
        if (playingMusicIds.ContainsKey(songType))
        {
            AudioManager.root.StopSound(songType);
            playingMusicIds.Remove(songType);
        }
    }

    public void SetState(AudioState stateType)
    {
        if (stateType != AudioState.None)
        {
            var separatedState = stateType.ToString().Split("_BREAK_");
            AkUnitySoundEngine.SetState(separatedState[0], separatedState[1]);
        }
    }

    public void SetTrigger(AudioTrigger triggerType)
    {
        if(triggerType != AudioTrigger.None)
            AkUnitySoundEngine.PostTrigger(triggerType.ToString(), gameObject);
    }
}