using System;
using System.Text.RegularExpressions;

public class BrPuzzle : Broadcaster
{
    public override void Awake()
    {
        RegisterActiveBroadcaster(this);
    }
    [AllowedStates(GameState.Synced, GameState.InPuzzle)]
    public override void OnNoteOn(int noteInput, int noteVeloctiy)
    {
        if (CheckReset(noteInput)) return;

        if (activePuzzle.solved >= activePuzzle.solutions.Length) return;

        if (
            activePuzzle.solutions[activePuzzle.solved].note.Pitch == noteInput &&
            (!activePlate.gems[activePuzzle.solved].needsLight || activePlate.gems[activePuzzle.solved].hasLight)
            )
            NextInSequence();
        else activePuzzle.solved = activePuzzle.FindLastCheckpoint();
    }
    bool CheckReset(int noteInput)
    {
        if (noteInput != 13)
        {
            activePuzzle.reset = 0;
            return false;
        }

        activePuzzle.reset++;

        if (activePuzzle.reset == 3) return true;

        return false;
    }

    void NextInSequence()
    {
        if (activePuzzle.solved == 0 && GameManager.root.State == GameState.Synced) activePlate.StartPuzzle();

        activePuzzle.solved++;
        activePuzzle.reset = 0;
        
        Match match = Regex.Match(activePlate.progressText.text, @"\d+");

        if(match.Success)
        {
            activePlate.progressText.text = 
            
            activePlate.progressText.text.Substring(0, match.Index) + activePuzzle.solved + activePlate.progressText.text.Substring(match.Index + match.Length);
        }

        if (activePuzzle.solved == activePuzzle.solutions.Length || activePuzzle.solutions[activePuzzle.solved].checkpoint)
        {
            for (int i = activePuzzle.FindLastCheckpoint(activePuzzle.solved - 1); i < activePuzzle.solved; i++)
            {
                var duration = activePuzzle.solutions[i].noteDuration.CurrentValue;
                MusicManager.root.currentSong.AddQueuedCallback($"{gameObject.name}SequenceSolved", duration, activePlate.gems[i].CheckpointReached);
            }
        }
    }

    
}