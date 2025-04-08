using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private Dictionary<(AudioEvent, GameObject, float), Queue<uint>> postedSoundEvents = new();

    void Start()
    {
        AkBankManager.LoadBank("Global", false, false);
        AkBankManager.LoadBank("Level01", false, false);
    }
    public bool PlaySound(AudioEvent soundType, GameObject soundSource = null, float instanceNumber = 1, bool overrideInstanceLock = false)
    {
        if (soundType != AudioEvent.None)
        {
            GameObject sourceObj = soundSource != null ? soundSource : gameObject;

            AkCallbackManager.EventCallback callback = (object inCookie, AkCallbackType type, AkCallbackInfo info) =>
            {
                if (type == AkCallbackType.AK_EndOfEvent)
                {
                    var key = (soundType, sourceObj, instanceNumber);

                    if (postedSoundEvents.ContainsKey(key))
                    {
                        postedSoundEvents[key].Dequeue();

                        if (postedSoundEvents[key].Count == 0)
                            postedSoundEvents.Remove(key);
                    }
                }
            };

            if (postedSoundEvents.ContainsKey((soundType, sourceObj, instanceNumber)) && !overrideInstanceLock)
            {
                return false;
            }
            else
            {
                uint eventId = AkUnitySoundEngine.PostEvent(soundType.ToString(), sourceObj, (uint)AkCallbackType.AK_EndOfEvent, callback, null);

                var key = (soundType, sourceObj, instanceNumber);
                if (!postedSoundEvents.ContainsKey(key))
                    postedSoundEvents[key] = new Queue<uint>();

                postedSoundEvents[key].Enqueue(eventId);

                return true;
            }
        }

        return false;
    }

    public bool StopSound(AudioEvent soundType, GameObject soundSource = null, float instanceNumber = 1)
    {
        if (soundType != AudioEvent.None)
        {
            GameObject sourceObj = soundSource != null ? soundSource : gameObject;

            if (postedSoundEvents.TryGetValue((soundType, sourceObj, instanceNumber), out var eventIdQueue))
            {
                foreach (uint eventId in eventIdQueue)
                {
                    AkUnitySoundEngine.StopPlayingID(eventId);
                }
                eventIdQueue.Clear();
                postedSoundEvents.Remove((soundType, soundSource, instanceNumber));

                return true;
            }
        }

        return false;
    }

    public bool IsPlaying(AudioEvent soundType, GameObject soundSource = null, float instanceNumber = 1)
    {
        if (postedSoundEvents.ContainsKey((soundType, soundSource != null ? soundSource : gameObject, instanceNumber)))
        {
            return true;
        }
        return false;
    }
    public void SetSwitch(AudioSwitch switchType, GameObject sourceObject = null)
    {
        if (switchType != AudioSwitch.None)
        {
            var separatedSwitch = switchType.ToString().Split("_BREAK_");
            AkUnitySoundEngine.SetSwitch(separatedSwitch[0], separatedSwitch[1], sourceObject != null ? sourceObject : gameObject);
        }
    }

    public void SetRTPC(AudioRTPC rtpcType, float value, bool isGlobal = true, AudioEvent localEvent = AudioEvent.None, GameObject sourceObject = null, float instanceNumber = 1)
    {
        if (rtpcType != AudioRTPC.None)
        {
            GameObject sourceObj = sourceObject != null ? sourceObject : gameObject;

            if (isGlobal)
            {
                AkUnitySoundEngine.SetRTPCValue(rtpcType.ToString(), value);
            }
            else
            {
                if (postedSoundEvents.TryGetValue((localEvent, sourceObj, instanceNumber), out var eventIdQueue))
                {
                    foreach (var eventId in eventIdQueue)
                    {
                        AkUnitySoundEngine.SetRTPCValueByPlayingID(rtpcType.ToString(), value, eventId);
                    }
                }
            }
        }
    }
    public float GetRTPC(AudioRTPC rtpcType, GameObject sourceObj = null)
    {
        int type = 1;

        AkUnitySoundEngine.GetRTPCValue(rtpcType.ToString(), (sourceObj != null) ? sourceObj : gameObject, 0, out var returnValue, ref type);
        return returnValue;
    }

    void OnDisable()
    {
        AkBankManager.UnloadBank("Global");
        AkBankManager.UnloadBank("Level01");
    }
}
