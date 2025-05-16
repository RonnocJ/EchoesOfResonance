using System.Collections;
using UnityEngine;

public class BrBattery : Broadcaster
{
    public float noteDownDrain;
    public float noteSustainDrain;
    public float finderDrain;
    public float chordDrain;
    public override void Awake()
    {
        base.Awake();
        RegisterActiveBroadcaster(this);

        obj = gameObject;
        OnHeldNotesEmptied += CheckRegen;
        OnBatteryEmpty += Shutdown; 
    }
    public override void OnNoteOn(int newNote, int velocity)
    {
        base.OnNoteOn(newNote, velocity);
        
        CRManager.Stop("Regen", this);

        batteryLevel -= noteDownDrain;
        batteryDrainAmount += noteSustainDrain;

        CRManager.Begin(DrainBatteryRoutine(), "DrainBattery", this);
    }
    public override void OnNoteOff(float oldNote)
    {
        base.OnNoteOff(oldNote);

        batteryDrainAmount -= noteSustainDrain;
    }
    public override void ModChange(float newModInput)
    {
        base.ModChange(newModInput);

        int newFinderLevel = Mathf.RoundToInt(((modInput * 1.25f) - 0.2f) / 0.25f);

        if(modInput > 0.2f)
        {
            CRManager.Stop("Regen", this);

            if(newFinderLevel != finderLevel)
            {
                batteryDrainAmount -= GetFinderDrain(finderLevel);
                batteryDrainAmount += GetFinderDrain(newFinderLevel);

                finderLevel = newFinderLevel;
            }

            CRManager.Begin(DrainBatteryRoutine(), "DrainBattery", this);
        }
        else
        {
            batteryDrainAmount -= GetFinderDrain(finderLevel);
            CheckRegen();
        }
    }
    public override void OnPuzzleSynced()
    {
        base.OnPuzzleSynced();
    }
    public override void OnPuzzleEnter()
    {
        base.OnPuzzleEnter();
    }
    public override void OnPuzzleExit()
    {
        base.OnPuzzleExit();
        batteryDrainAmount -= GetFinderDrain(finderLevel);
    }
    private float GetFinderDrain(int newFinderLevel)
    {
        return Mathf.Pow(newFinderLevel, 2) * finderDrain;
    }
    private void CheckRegen()
    {
        if(modInput < 0.2f || GameManager.root.State != GameState.InPuzzle)
            CRManager.Restart(RegenRoutine(), "Regen", this);

    }
    private void Shutdown()
    {
        CRManager.Begin(ShutdownRoutine(), "Shutdown", this);
    }
    public IEnumerator DrainBatteryRoutine()
    {
        while (draining)
        {
            batteryLevel -= batteryDrainAmount * Time.deltaTime;
            yield return null;
        } 
    }
    IEnumerator RegenRoutine()
    {
        float regenMultiplier = 1;
        yield return new WaitForSeconds(1.5f);
        while (batteryLevel < 1f && !draining)
        {
            batteryLevel += regenMultiplier * 0.05f * Time.deltaTime;
            regenMultiplier *= 1.005f;
            yield return null;
        }
    }
    IEnumerator ShutdownRoutine()
    {
        GameManager.root.State = GameState.Shutdown;

        InputManager.root.AllNotesOff();
        
        if(activePlate != null) activePlate.EjectPlayer();

        CRManager.Stop("Regen", this);
        CRManager.Stop("DrainBatteryNote", this);

        yield return new WaitForSeconds(5f);

        batteryLevel = 1f;

        OnBatteryCharge?.Invoke();
        
        GameManager.root.State = GameState.Roaming;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        OnHeldNotesEmptied -= CheckRegen;
        OnBatteryEmpty -= Shutdown; 
    }
}