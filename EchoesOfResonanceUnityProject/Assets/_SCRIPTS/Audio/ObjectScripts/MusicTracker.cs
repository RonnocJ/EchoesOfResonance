using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Objects/Audio/MusicTracker", order = 1)]
public class MusicTracker : ScriptableObject
{
    public float grid;
    public int length;
    public Dictionary<float, Action<float>> callbackFrequency = new();
    private AkMusicSyncCallbackInfo musicInfo;
    public void SetTracker()
    {
        grid = 0;
        length = 0;
    }
    public void AddBeatListener(float duration, Action<float> action)
    {
        if(!callbackFrequency.ContainsKey(duration))
        {
            callbackFrequency[duration] = new Action<float> (_ => { });
        }

        callbackFrequency[duration] += action;
    }
    public void RemoveBeatListener(float duration, Action<float> action)
    {
        callbackFrequency[duration] -= action;
    }
    public void MusicCallbackFunction(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
    {
        if (in_info is AkMusicSyncCallbackInfo _musicInfo)
        {
            musicInfo = _musicInfo;

            if (length == 0)
                length = Mathf.FloorToInt(musicInfo.segmentInfo_iActiveDuration / (musicInfo.segmentInfo_fBeatDuration * 1000));

            if (in_type == AkCallbackType.AK_MusicSyncGrid)
            {
                grid = (grid + 0.25f) % length;

                foreach(float duration in callbackFrequency.Keys)
                {
                    if(grid % duration == 0)
                    {
                        callbackFrequency[duration].Invoke(duration);
                    }
                }
            }
        }
    }

    public float GetBeatInSeconds()
    {
        return musicInfo.segmentInfo_fBeatDuration;
    }
}
