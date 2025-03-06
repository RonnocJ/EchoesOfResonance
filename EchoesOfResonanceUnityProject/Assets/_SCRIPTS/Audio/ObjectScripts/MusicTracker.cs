using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Audio/MusicTracker", order = 1)]
public class MusicTracker : ScriptableObject
{
    public class CallbackEntry
    {
        public float TimeToNextBeat;
        public Queue<(Action nextAction, float beatDelay)> CallbackQueue;

        public CallbackEntry()
        {
            CallbackQueue = new();
            TimeToNextBeat = 0f;
        }
    }
    public Dictionary<string, CallbackEntry> LoopingCallback = new();
    public Dictionary<string, CallbackEntry> QueuedCallback = new();
    private AkMusicSyncCallbackInfo _musicInfo;
    private readonly object _lock = new();

    private void AddCallback(Dictionary<string, CallbackEntry> callbackDict, string key, float beatDelay, Action action)
    {
        lock (_lock)
        {
            if (!callbackDict.TryGetValue(key, out var entry))
            {
                entry = new CallbackEntry();
                callbackDict[key] = entry;
            }
            entry.CallbackQueue.Enqueue((action, beatDelay));
        }
    }

    public void AddLoopingCallback(string loopName, float beatDelay, Action action)
    {
        AddCallback(LoopingCallback, loopName, beatDelay, action);
    }

    public void AddQueuedCallback(string queueName, float beatDelay, Action action)
    {
        AddCallback(QueuedCallback, queueName, beatDelay, action);
    }

    public void MusicCallbackFunction(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
    {
        if (in_info is AkMusicSyncCallbackInfo __musicInfo)
        {
            _musicInfo = __musicInfo;

            if (in_type == AkCallbackType.AK_MusicSyncGrid)
            {
                lock (_lock)
                {
                    ProcessCallbacks(LoopingCallback, (entry, action, delay) =>
                    {
                        entry.CallbackQueue.Enqueue((action, delay));
                    });

                    ProcessCallbacks(QueuedCallback, (entry, action, delay) => { });
                }
            }
        }
    }

    private void ProcessCallbacks(Dictionary<string, CallbackEntry> callbackDict, Action<CallbackEntry, Action, float> onActionExecuted)
    {
        var keysToRemove = new List<string>();

        foreach (var kvp in callbackDict)
        {
            var entry = kvp.Value;
            if (entry == null || entry.CallbackQueue == null || entry.CallbackQueue.Count == 0)
                continue;

            entry.TimeToNextBeat += 0.25f;

            if (entry.TimeToNextBeat >= entry.CallbackQueue.Peek().beatDelay)
            {
                entry.TimeToNextBeat = 0f;
                var (action, delay) = entry.CallbackQueue.Dequeue();
                action?.Invoke();
                onActionExecuted?.Invoke(entry, action, delay);

                if (entry.CallbackQueue.Count == 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
        }

        foreach (string key in keysToRemove)
        {
            callbackDict.Remove(key);
        }
    }

    public float GetBeatInSeconds()
    {
        return _musicInfo.segmentInfo_fBeatDuration;
    }
}