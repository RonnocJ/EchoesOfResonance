using UnityEngine;

public class BrChord : Singleton<BrChord>, IInputScript
{
    public ChordAbilities[] abilities;
    public void AddInputs()
    {
        InputManager.root.AddListener<float>(ActionTypes.ChordDown, PlayChordAbility);
    }

    void PlayChordAbility(float chordIndex)
    {
        switch(chordIndex)
        {
            case 0:
                Debug.Log("Played chord 1");
                break;
            case 1:
                Debug.Log("Played chord 2");
                break;
        }
    }
}