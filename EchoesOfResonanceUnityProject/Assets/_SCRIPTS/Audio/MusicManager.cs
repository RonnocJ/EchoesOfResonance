using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
public class MusicManager : Singleton<MusicManager>, ISaveData
{
    [SerializeField] private GameObject _configBG, _titleCard;
    [SerializeField] private MusicTracker _defaultSong;
    public MusicTracker currentSong, loadedSong;
    public GameState GameStateToSet;
    private Dictionary<AudioEvent, uint> playingMusicIds = new();
    private MusicTracker lastSongData;
    private Dictionary<string, AudioState> lastState = new();
    private Dictionary<string, AudioState> currentState = new();
    public Dictionary<string, object> AddSaveData()
    {
        var returnDict = new Dictionary<string, object>();

        returnDict["lastSong"] = $"Objects/Music/{lastSongData.name}";

        foreach (var state in lastState)
        {
            returnDict[$"stateGroup{state.Key}"] = state.Value;
        }

        return returnDict;
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        AkUnitySoundEngine.StopAll();

        AkUnitySoundEngine.PostEvent(AudioEvent.muteSFX.ToString(), gameObject);
        AkUnitySoundEngine.PostEvent(AudioEvent.playBasicAmbience.ToString(), gameObject);

        loadedSong = _defaultSong;

        if (savedData.TryGetValue("lastSong", out object lastSongRaw))
        {
            loadedSong = Resources.Load<MusicTracker>(Convert.ToString(lastSongRaw)) ?? _defaultSong;
        }

        lastSongData = loadedSong;

        if(loadedSong == _defaultSong)
        {
            lastState["Opening01"] = AudioState.Opening01_BREAK_Intro;
        }

        foreach (var entry in savedData)
        {
            if (entry.Key.StartsWith("stateGroup"))
            {
                if (Enum.TryParse(Convert.ToString(entry.Value), out AudioState savedState))
                {
                    var sepState = savedState.ToString().Split("_BREAK_");
                    lastState[sepState[0]] = savedState;
                }
            }
        }
    }
    public void MusicToGameplay()
    {
        PlaySong(loadedSong);

        if (!DH.Get<TestOverrides>().skipIntro)
        {
            SetState(currentSong.ToGameplayState);

            currentSong.AddQueuedCallback("SetMusicStateGameplay", 0.5f, () => new Dictionary<string, AudioState>(lastState).Values.ToList().ForEach(c => SetState(c)));
            currentSong.AddQueuedCallback("SyncGameplayTransition", currentSong.PreTitleLength, () =>
                {
                    UIUtil.root.SetAlpha(1, new() {_titleCard });
                });
            currentSong.AddQueuedCallback("SyncGameplayTransition", currentSong.PostTitleLength, () =>
                {
                    CRManager.Begin(UIUtil.root.FadeItems(0.06f, 0, true, new() { _configBG, _titleCard }), "FadeIntroScreen", this);
                    GameManager.root.State = GameStateToSet;
                    AkUnitySoundEngine.PostEvent(AudioEvent.unmuteSFX.ToString(), gameObject);
                });
        }
        else
        {
            new Dictionary<string, AudioState>(lastState).Values.ToList().ForEach(c => SetState(c));
            UIUtil.root.SetAlpha(0, new() { _configBG, _titleCard });
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
                lastSongData = newSong;
            }
        }
    }

    public void StopSong()
    {
        if (currentSong != null)
        {
            AkUnitySoundEngine.StopPlayingID(playingMusicIds[currentSong.MusicEvent]);
            AkUnitySoundEngine.StopPlayingID(playingMusicIds[currentSong.MetronomeEvent]);

            playingMusicIds.Remove(currentSong.MusicEvent);
            playingMusicIds.Remove(currentSong.MetronomeEvent);

            lastState.Clear();
            currentState.Clear();
        }
    }

    public void SetState(AudioState stateType)
    {
        if (stateType != AudioState.None)
        {
            ProcessCurrentState(stateType);

            if (!(GameManager.root.State is GameState.Cutscene or GameState.InPuzzle) && stateType != currentSong.ToGameplayState)
            {
                ProcessLastState(stateType);
            }

            var separatedState = stateType.ToString().Split("_BREAK_");
            AkUnitySoundEngine.SetState(separatedState[0], separatedState[1]);
        }
    }
    void ProcessCurrentState(AudioState inState)
    {
        var sepState = inState.ToString().Split("_BREAK_");

        if(currentState.ContainsKey(sepState[0]))
        {
            currentState[sepState[0]] = inState;
            return;
        }

        currentState[sepState[0]] = inState;
    }
    void ProcessLastState(AudioState inState)
    {
        var sepState = inState.ToString().Split("_BREAK_");

        if(lastState.ContainsKey(sepState[0]))
        {
            lastState[sepState[0]] = inState;
            return;
        }

        lastState[sepState[0]] = inState;
    }

    public void SetTrigger(AudioTrigger triggerType)
    {
        if (triggerType != AudioTrigger.None)
            AkUnitySoundEngine.PostTrigger(triggerType.ToString(), gameObject);
    }
    public void RefreshMusicData()
    {
        lastSongData = currentSong;
        currentState.Values.ToList().ForEach(c => ProcessLastState(c));
    }

    void OnDestroy()
    {
        var songs = Resources.LoadAll<MusicTracker>("");

        foreach (var s in songs)
        {
            s.LoopingCallback.Clear();
            s.QueuedCallback.Clear();
        }
    }
}