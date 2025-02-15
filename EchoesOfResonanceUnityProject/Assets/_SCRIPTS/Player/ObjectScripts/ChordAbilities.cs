using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Player/ChordAbilities", order = 1)]
public class ChordAbilities : ScriptableObject
{
    public bool unlocked;
    public string[] notes;
}