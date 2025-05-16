

/// <summary>
///   The list of events in the game.
/// </summary>
public enum AudioEvent {
    None = 0,
    muteSFX = -1821269725,
    pauseAll = -203920114,
    playBasicAmbience = 1434361023,
    playBroadcasterFinder = -344641179,
    playBroadcasterFX = 1668433167,
    playBroadcasterNote = -700603383,
    playCheckpointReached = 1481495309,
    playFootsteps = 1088348632,
    playGemHum = -1606603762,
    playShutoff = 1900847084,
    playTextBeep = -1104122518,
    playTorchLitLoop = 1496897584,
    resumeAll = -1054066427,
    stopBroadcasterFX = -2033717619,
    stopGemHum = -1352214320,
    unmuteSFX = -251888464,
    endOpening = 161092132,
    playMonitorTick = 19295944,
    playRingTick = 2050963566,
    startMetronome01Opening = -354731248,
    startMetronome01Puzzle = -200379460,
    startMusic01Opening = -538394609,
    startMusic01Puzzle = 676444101,
}

/// <summary>
///   The list of states in the game.
/// </summary>
public enum AudioState {
    None = 0,
    _RingPuzzle_BREAK_Checkpoint1 = 225972860,
    _RingPuzzle_BREAK_Checkpoint3 = 225972862,
    _RingPuzzle_BREAK_Checkpoint2 = 225972863,
    _RingPuzzle_BREAK_None = 748895195,
    _RingPuzzle_BREAK_Start = 1281810935,
    Puzzle01_BREAK_None = 748895196,
    Puzzle01_BREAK_ToGameplay = 1824561598,
    Puzzle01_BREAK_RingPuzzle = -1579243349,
    Opening01_BREAK_None = 748895197,
    Opening01_BREAK_Intro = 1125500713,
    Opening01_BREAK_BroadcasterApproach = 1253314903,
    Opening01_BREAK_ToGameplay = 1824561599,
    Opening01_BREAK_GetBroadcaster = -1274687595,
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
}

/// <summary>
///   The list of rtpcs in the game.
/// </summary>
public enum AudioRTPC {
    None = 0,
    flute_Velocity = 350681501,
    broadcaster_Shutdown = 653430204,
    music_Volume = 1006694123,
    finder_Pitch = 1152538158,
    gemCheckpoint_Pitch = 1472233667,
    sfx_Volume = 1564184899,
    player_MoveSpeed = 1786911663,
    ambience_Ducking = 1899861129,
    gemHum_Pitch = -1867120053,
    flute_Pitch = -885794186,
}