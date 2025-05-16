public class BrAudio: Broadcaster
{
    public override void Awake()
    {
        RegisterActiveBroadcaster(this);
        OnBatteryEmpty += ShutdownAudio;
        OnBatteryCharge += RebootAudio;
    }
    [AllowedStates(GameState.InPuzzle, GameState.Roaming, GameState.Synced)]
    public override void OnNoteOn(int newNote, int velocity)
    {
        AudioManager.root.StopSound(AudioEvent.playBroadcasterFinder, gameObject, 1);
        AudioManager.root.PlaySound(AudioEvent.playBroadcasterFX, obj, 1);
        AudioManager.root.PlaySound(AudioEvent.playBroadcasterNote, obj, newNote);
        AudioManager.root.SetRTPC(AudioRTPC.flute_Pitch, newNote, false, AudioEvent.playBroadcasterNote, obj, newNote);
        AudioManager.root.SetRTPC(AudioRTPC.flute_Velocity, velocity, false, AudioEvent.playBroadcasterNote, obj, newNote);     
    }

    [AllowAllAboveState(GameState.InPuzzle), DissallowedStates(GameState.Intro)]
    public override void OnNoteOff(float oldNote)
    {
        AudioManager.root.StopSound(AudioEvent.playBroadcasterNote, obj, (int)oldNote);
        
        if(heldNotes.Count == 0 && modInput > 0.2f && GameManager.root.State == GameState.InPuzzle) 
            AudioManager.root.PlaySound(AudioEvent.playBroadcasterFinder, gameObject, 1);
    }
    [AllowedStates(GameState.InPuzzle)]
    public override void ModChange(float newModInput)
    {
        if(modInput > 0.2f && activePuzzle.solutions.Length > activePuzzle.solved)
            AudioManager.root.PlaySound(AudioEvent.playBroadcasterFinder, gameObject, 1);
        else
            AudioManager.root.StopSound(AudioEvent.playBroadcasterFinder, gameObject, 1);
    }
    public override void OnPuzzleExit()
    {
        AudioManager.root.StopSound(AudioEvent.playBroadcasterFinder, gameObject, 1);
    }
    public void ShutdownAudio()
    {
        AudioManager.root.StopSound(AudioEvent.playBroadcasterFinder, gameObject, 1);
        AudioManager.root.StopSound(AudioEvent.playBroadcasterFX, gameObject, 1);
        AudioManager.root.PlaySound(AudioEvent.playShutoff, gameObject);
        AudioManager.root.SetRTPC(AudioRTPC.broadcaster_Shutdown, 100);
    }
    public void RebootAudio()
    {
        AudioManager.root.SetRTPC(AudioRTPC.broadcaster_Shutdown, 0);
    }

    public override void OnDestroy()
    {
        OnBatteryEmpty -= ShutdownAudio;
        OnBatteryCharge -= RebootAudio;
    }
}