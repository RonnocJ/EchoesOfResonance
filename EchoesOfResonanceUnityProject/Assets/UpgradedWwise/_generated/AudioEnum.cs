

/// <summary>
///   The list of events in the game.
/// </summary>
public enum AudioEvent {
    None = 0,
    pauseAll = -203920114,
    playBroadcasterBeep = -1885145075,
    playBroadcasterBloop = 893306899,
    playBroadcasterFinder = -344641179,
    playBroadcasterFX = 1668433167,
    playBroadcasterNote = -700603383,
    playBroadcasterPlunk = 879710867,
    playCheckpointReached = 1481495309,
    playDoorOpen = -655044497,
    playFootsteps = 1088348632,
    playIntroAmbience = -910042053,
    playShutoff = 1900847084,
    playStoneCrumble = 68937622,
    playTorchExtinguish = -615065405,
    playTorchIgnite = 1562608897,
    resumeAll = -1054066427,
    stopBroadcasterFX = -2033717619,
    stopIntroAmbience = 941857173,
    stopTone = 2019189489,
    playBoulderChime1 = 2019948791,
    playBoulderChime2 = 2019948788,
    playBoulderChime3 = 2019948789,
    playRingMoveTick = 1201866565,
    playToGameplayTransition01 = -2112098624,
    startMetronome01 = -942943184,
    startMusic01 = -776453267,
}

/// <summary>
///   The list of states in the game.
/// </summary>
public enum AudioState {
    None = 0,
    EntranceHall_BREAK_None = 748895195,
    EntranceHall_BREAK_Base = 1291433366,
    EntranceHall_BREAK_Puzzle04 = -1027100335,
    EntranceHall_BREAK_Puzzle01 = -1027100332,
    EntranceHall_BREAK_Puzzle03 = -1027100330,
    EntranceHall_BREAK_Puzzle02 = -1027100329,
    Level01Master_BREAK_EntranceHall = 270858878,
    Level01Master_BREAK_None = 748895195,
    Level01Master_BREAK_Opening = 1831982039,
    Level01Master_BREAK_GetBroadcaster = -1274687595,
}

/// <summary>
///   The list of switches in the game.
/// </summary>
public enum AudioSwitch {
    None = 0,
}

/// <summary>
///   The list of triggers in the game.
/// </summary>
public enum AudioTrigger {
    None = 0,
    HintDelay = -1768730501,
}

/// <summary>
///   The list of rtpcs in the game.
/// </summary>
public enum AudioRTPC {
    None = 0,
    broadcaster_Shutdown = 653430204,
    music_Volume = 1006694123,
    finder_Pitch = 1152538158,
    introBridge_StepDuck = 1325667752,
    sfx_Volume = 1564184899,
    firstElevator_Height = 1867101979,
    introBridge_CrossBridge = -1568484832,
    cliffsEdge_Fade = -1325370576,
    introBridge_FadeIn = -1077959362,
    flute_Pitch = -885794186,
}