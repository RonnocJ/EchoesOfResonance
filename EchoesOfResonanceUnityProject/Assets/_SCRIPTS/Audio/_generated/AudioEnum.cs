

/// <summary>
///   The list of events in the game.
/// </summary>
public enum AudioEvent {
    None = 0,
    activateStepDucking = -946470267,
    pauseAll = -203920114,
    playBroadcasterBeep = -1885145075,
    playBroadcasterBloop = 893306899,
    playBroadcasterFX = 1668433167,
    playBroadcasterNote = -700603383,
    playBroadcasterPlunk = 879710867,
    playCheckpointReached = 1481495309,
    playDoorOpen = -655044497,
    playElevatorHints = -1041904867,
    playEndCutscene = -984282170,
    playFootsteps = 1088348632,
    playIntroAmbience = -910042053,
    playShutoff = 1900847084,
    playTorchExtinguish = -615065405,
    playTorchIgnite = 1562608897,
    resumeAll = -1054066427,
    startIntroBridgeChoir = 1116304881,
    startMetronome01 = -942943184,
    startMusic01 = -776453267,
    stopBroadcasterFX = -2033717619,
    stopIntroAmbience = 941857173,
    stopIntroBridgeChoir = -1921443197,
    stopTone = 2019189489,
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
    Level01Master_BREAK_Intro = 1125500713,
    Level01Master_BREAK_CliffsEdge = 1149990767,
    Level01Master_BREAK_ElevatorShaft = 1318468365,
    Level01Master_BREAK_ElevatorHallway = 1379644315,
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
    ToG = 1080872029,
    BridgeCrossing02 = 1580584756,
    BridgeCrossing01 = 1580584759,
    HintDelay = -1768730501,
    ToBb = -1018843414,
}

/// <summary>
///   The list of rtpcs in the game.
/// </summary>
public enum AudioRTPC {
    None = 0,
    midi_Pitch = 831692701,
    music_Volume = 1006694123,
    introBridge_StepDuck = 1325667752,
    sfx_Volume = 1564184899,
    firstElevator_Height = 1867101979,
    introBridge_CrossBridge = -1568484832,
    introBridge_FadeIn = -1077959362,
}