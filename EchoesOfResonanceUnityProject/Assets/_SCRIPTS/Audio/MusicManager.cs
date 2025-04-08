using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
public class MusicManager : Singleton<MusicManager>, ISaveData
{
    [SerializeField] private GameObject _configBG;
    [SerializeField] private MusicTracker _defaultSong;
    public MusicTracker currentSong;
    public GameState GameStateToSet;
    private readonly Dictionary<AudioEvent, AudioEvent> musicMetronomeRef = new()
    {
        {AudioEvent.startMusic01, AudioEvent.startMetronome01}
    };
    private Dictionary<AudioEvent, uint> playingMusicIds = new();
    private AudioEvent lastSongEvent;
    private AudioState lastState;
    public Dictionary<string, object> AddSaveData()
    {
        return new()
        {
            {"lastSong", (lastSongEvent, currentSong.name)}, {"lastState", lastState}
        };
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        lastState = AudioState.Level01Master_BREAK_Opening;

        if (savedData.TryGetValue("lastState", out object lastStateRaw))
        {
            Enum.TryParse(Convert.ToString(lastStateRaw), out AudioState lastStateData);
            lastState = lastStateData;
        }

        lastSongEvent = AudioEvent.startMusic01;
        currentSong = _defaultSong;

        if (savedData.TryGetValue("lastSong", out object lastSongRaw))
        {
            string json = JsonConvert.SerializeObject(lastSongRaw);
            (string audioEventStr, string trackerName) = JsonConvert.DeserializeObject<(string, string)>(json);

            Enum.TryParse(audioEventStr, out lastSongEvent);
            currentSong = Resources.Load<MusicTracker>(trackerName) ?? _defaultSong;
        }
    }
    public void MusicToGameplay()
    {
        if (DH.Get<TestOverrides>().skipIntro)
        {
            PlaySong(lastSongEvent, currentSong);
            SetState(lastState);
            UIFade.root.SetAlpha(0, new() { _configBG });
            GameManager.root.currentState = GameStateToSet;
        }
        else
        {
            AkCallbackManager.EventCallback callback = (object inCookie, AkCallbackType type, AkCallbackInfo info) =>
                {
                    if (type == AkCallbackType.AK_MusicSyncExit)
                    {
                        PlaySong(lastSongEvent, currentSong);
                        SetState(lastState);
                        UIFade.root.SetAlpha(0, new() { _configBG });
                        GameManager.root.currentState = GameStateToSet;
                    }
                };

            AkUnitySoundEngine.PostEvent(AudioEvent.playToGameplayTransition01.ToString(), gameObject, (uint)AkCallbackType.AK_MusicSyncExit, callback, null);
        }
    }
    public void PlaySong(AudioEvent songType, MusicTracker newSong)
    {
        if (!playingMusicIds.ContainsKey(songType) && musicMetronomeRef.TryGetValue(songType, out AudioEvent metronome) && songType != AudioEvent.None)
        {
            lastSongEvent = songType;
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
            if (!(GameManager.root.currentState is GameState.Cutscene or GameState.InPuzzle))
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