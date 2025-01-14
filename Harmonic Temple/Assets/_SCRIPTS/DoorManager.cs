using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public DoorData currentDoor;
    public GameObject currentDoorObj;

    public void CheckNote(string newNote)
    {
        int rightNoteCheck = currentDoor.solved;
        foreach (var data in currentDoor.solutions)
        {
            if (newNote == data.correctNote)
            {
                data.noteHeld = true;
                currentDoor.solved++;
            }
        }

        if(rightNoteCheck == currentDoor.solved)
        {
            currentDoor.solved--;
        }

        if(currentDoor.solved == currentDoor.solutions.Length)
        {
            currentDoorObj.GetComponent<Animator>().SetTrigger("openDoor");
        }
    }
    public void RemoveNote(string oldNote)
    {
        foreach (var data in currentDoor.solutions)
        {
            if (oldNote == data.correctNote)
            {
                data.noteHeld = false;
                currentDoor.solved--;
            }
        }
    }
}
