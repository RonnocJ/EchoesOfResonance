using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Audio/MusicTracker", order = 1)]
public class MusicTracker : ScriptableObject
{
    public AK.Wwise.Event musicEvent;
    public int beat;
    public int bar;
    public int grid;
    public int length;
    public Action OnEveryBeat, OnEveryBar, OnEveryGrid, OnEveryLoop;
    public void StartMusic(GameObject obj)
    {
        beat = 0;
        bar = 0;

        OnEveryBeat = new Action(() => { });
        OnEveryBar = new Action(() => { });
        OnEveryGrid = new Action(() => { });
        OnEveryLoop = new Action(() => { });

        musicEvent.Post(obj, (uint)(AkCallbackType.AK_MusicSyncAll | AkCallbackType.AK_EnableGetMusicPlayPosition), MusicCallbackFunction);
    }

    void MusicCallbackFunction(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
    {
        if (in_info is AkMusicSyncCallbackInfo _musicInfo)
        {
            if (length == 0)
                length = Mathf.RoundToInt(_musicInfo.segmentInfo_iActiveDuration / _musicInfo.segmentInfo_fBarDuration);

            switch (_musicInfo.musicSyncType)
            {
                case AkCallbackType.AK_MusicSyncBeat:
                    beat = (beat % 4) + 1;
                    OnEveryBeat.Invoke();
                    break;
                case AkCallbackType.AK_MusicSyncBar:
                    bar = (bar % length) + 1;
                    OnEveryBar.Invoke();
                    break;
                case AkCallbackType.AK_MusicSyncGrid:
                    OnEveryGrid.Invoke();
                    break;
                case AkCallbackType.AK_MusicSyncExit:
                    OnEveryLoop.Invoke();
                    break;
            }
        }
    }
}
