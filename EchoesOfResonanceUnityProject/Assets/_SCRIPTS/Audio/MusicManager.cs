using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
public class MusicManager : Singleton<MusicManager>, ISaveData
{
    [SerializeField] private GameObject _configBG;
    [SerializeField] private MusicTracker _defaultSong;
    public MusicTracker currentSong, loadedSong;
    public GameState GameStateToSet;
    private Dictionary<AudioEvent, uint> playingMusicIds = new();
    private AudioEvent lastSongEvent;
    private MusicTracker lastSongData;
    private AudioState lastState;
    public Dictionary<string, object> AddSaveData()
    {
        return new()
        {
            {"lastSong", (lastSongEvent, lastSongData.name)}, {"lastState", lastState}
        };
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        AkUnitySoundEngine.PostEvent(AudioEvent.muteSFX.ToString(), gameObject);
        AkUnitySoundEngine.PostEvent(AudioEvent.playBasicAmbience.ToString(), gameObject);

        lastState = AudioState.Opening01_BREAK_Opening;

        if (savedData.TryGetValue("lastState", out object lastStateRaw))
        {
            Enum.TryParse(Convert.ToString(lastStateRaw), out AudioState lastStateData);
            lastState = lastStateData;
        }

        lastSongEvent = AudioEvent.startMusic01Opening;
        loadedSong = _defaultSong;

        if (savedData.TryGetValue("lastSong", out object lastSongRaw))
        {
            string json = JsonConvert.SerializeObject(lastSongRaw);
            (string audioEventStr, string trackerName) = JsonConvert.DeserializeObject<(string, string)>(json);

            Enum.TryParse(audioEventStr, out lastSongEvent);
            loadedSong = Resources.Load<MusicTracker>(trackerName) ?? _defaultSong;
        }

        lastSongData = loadedSong;
    }
    public void MusicToGameplay()
    {
        PlaySong(loadedSong);

        if (!DH.Get<TestOverrides>().skipIntro)
        {
            SetState(currentSong.ToGameplayState);
            currentSong.AddQueuedCallback("SetMusicStateGameplay", 0.5f, () => SetState(lastState));

            currentSong.AddQueuedCallback("SyncGameplayTransition", currentSong.IntroLength, () =>
                {
                    UIUtil.root.SetAlpha(0, new() { _configBG });
                    GameManager.root.State = GameStateToSet;
                    AkUnitySoundEngine.PostEvent(AudioEvent.unmuteSFX.ToString(), gameObject);
                });
        }
        else
        {
            SetState(lastState);
            UIUtil.root.SetAlpha(0, new() { _configBG });
            GameManager.root.State = GameStateToSet;
            AkUnitySoundEngine.PostEvent(AudioEvent.unmuteSFX.ToString(), gameObject);
        }
    }
    public void PlaySong(MusicTracker newSong)
    {
        StopSong();

        if (!playingMusicIds.ContainsKey(newSong.MusicEvent))
        {
            currentSong = newSong;

            playingMusicIds[newSong.MusicEvent] = AkUnitySoundEngine.PostEvent(newSong.MusicEvent.ToString(), gameObject);
            playingMusicIds[newSong.MetronomeEvent] = AkUnitySoundEngine.PostEvent(newSong.MetronomeEvent.ToString(), gameObject, (uint)(AkCallbackType.AK_MusicSyncAll | AkCallbackType.AK_EnableGetMusicPlayPosition), newSong.MusicCallbackFunction, null);

            currentSong.Grid = 0;

            if (!(GameManager.root.State is GameState.Cutscene or GameState.InPuzzle))
            {
                lastSongEvent = newSong.MusicEvent;
                lastSongData = newSong;
            }
        }
    }

    public void StopSong()
    {
        if (currentSong != null)
        {
            AkUnitySoundEngine.StopPlayingID(playingMusicIds[lastSongEvent]);
            AkUnitySoundEngine.StopPlayingID(playingMusicIds[currentSong.MetronomeEvent]);
    
            playingMusicIds.Remove(lastSongEvent);
            playingMusicIds.Remove(currentSong.MetronomeEvent);
    
            lastSongEvent = AudioEvent.None;
            lastState = AudioState.None;
        }
    }

    public void SetState(AudioState stateType)
    {
        if (stateType != AudioState.None)
        {
            if (!(GameManager.root.State is GameState.Cutscene or GameState.InPuzzle) && stateType != currentSong.ToGameplayState)
            {
                lastState = stateType;
            }

            var separatedState = stateType.ToString().Split("_BREAK_");
            AkUnitySoundEngine.SetState(separatedState[0], separatedState[1]);
        }
    }

    public void SetTrigger(AudioTrigger triggerType)
    {
        if (triggerType != AudioTrigger.None)
            AkUnitySoundEngine.PostTrigger(triggerType.ToString(), gameObject);
    }
}