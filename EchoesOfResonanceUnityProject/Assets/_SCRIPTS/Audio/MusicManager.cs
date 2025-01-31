using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MusicManager : Singleton<MusicManager>
{
    void Start()
    {
        var puzzles = Resources.LoadAll<PuzzleData>("");

        foreach(var p in puzzles)
        {
            p.OnPuzzleCompleted += () => p.SetMusicComplete();
        }
    }
}