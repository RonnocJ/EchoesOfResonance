using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private Dictionary<(AudioEvent, GameObject, float), uint> postedSoundEvents = new();

    public bool PlaySound(AudioEvent soundType, GameObject soundSource = null, float instanceNumber = 1)
    {
        if (soundType != AudioEvent.None)
        {
            GameObject sourceObj = soundSource != null ? soundSource : gameObject;

            if (postedSoundEvents.ContainsKey((soundType, sourceObj, instanceNumber)))
            {
                return false;
            }

            AkCallbackManager.EventCallback callback = (object inCookie, AkCallbackType type, AkCallbackInfo info) =>
            {
                if (type == AkCallbackType.AK_EndOfEvent)
                {
                    postedSoundEvents.Remove((soundType, sourceObj, instanceNumber));
                }
            };

            if (!postedSoundEvents.ContainsKey((soundType, sourceObj, instanceNumber)))
            {
                uint eventId = AkUnitySoundEngine.PostEvent(soundType.ToString(), sourceObj, (uint)AkCallbackType.AK_EndOfEvent, callback, null);
                postedSoundEvents[(soundType, sourceObj, instanceNumber)] = eventId;

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

            if (postedSoundEvents.TryGetValue((soundType, sourceObj, instanceNumber), out uint eventId))
            {
                AkUnitySoundEngine.StopPlayingID(eventId);
                return true;
            }
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
                if (postedSoundEvents.TryGetValue((localEvent, sourceObj, instanceNumber), out uint eventId))
                {
                    AkUnitySoundEngine.SetRTPCValueByPlayingID(rtpcType.ToString(), value, eventId);
                }
            }
        }
    }
}
