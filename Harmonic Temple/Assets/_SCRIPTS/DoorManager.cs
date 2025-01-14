using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [SerializeField] private float timeLimit, gemIntensity;
    [SerializeField] private Transform doorList;
    [SerializeField] private DoorData[] dataList;
    public DoorData currentDoorData;
    public GameObject currentDoorObj;
    private float timer;
    [SerializeField] private Material hourglassMat;
    private List<Transform> currentDoorGems = new List<Transform>();
    private List<Material> gemMats = new List<Material>();
    void Start()
    {
        timer = timeLimit;
        currentDoorData = dataList[0];
        currentDoorObj = doorList.GetChild(0).gameObject;
        EstablishDoor();
    }
    private void EstablishDoor()
    {
        currentDoorGems.Clear();
        gemMats.Clear();
        currentDoorData.solved = 0;

        for (int i = 0; i < currentDoorData.solutions.Length; i++)
        {
            currentDoorGems.Add(currentDoorObj.transform.GetChild(i + 1));
            gemMats.Add(currentDoorGems[i].GetComponent<MeshRenderer>().material);
            gemMats[i].SetColor("_BaseColor", currentDoorData.solutions[i].gemColor);
            gemMats[i].SetColor("_EmissionColor", Vector4.zero);
        }
    }
    void Update()
    {
        if (timer > 0)
        {
            hourglassMat.SetFloat("_timeRemaining", timer / timeLimit * 2f);
            timer -= Time.deltaTime;
        }
    }
    public void CheckNote(string newNote)
    {
        bool isCorrectNote = false;
        for (int i = 0; i < currentDoorData.solutions.Length; i++)
        {
            if (newNote == currentDoorData.solutions[i].correctNote)
            {
                isCorrectNote = true;
                if (!currentDoorData.solutions[i].noteHeld)
                {
                    currentDoorData.solutions[i].noteHeld = true;
                    currentDoorData.solved++;
                    gemMats[i].EnableKeyword("_EMISSION");
                    gemMats[i].SetColor("_EmissionColor", currentDoorData.solutions[i].gemColor * gemIntensity);
                }
            }
        }

        if (!isCorrectNote) currentDoorData.solved--;

        if (currentDoorData.solved == currentDoorData.solutions.Length)
        {
            currentDoorObj.GetComponent<Animator>().SetTrigger("openDoor");
            currentDoorData = dataList[currentDoorObj.transform.GetSiblingIndex() + 1];
            currentDoorObj = doorList.GetChild(currentDoorObj.transform.GetSiblingIndex() + 1).gameObject;
            EstablishDoor();
            GetComponent<NoteManager>().AllNotesOff();
        }
    }
    public void RemoveNote(string oldNote)
    {
        bool isCorrectNote = false;
        for (int i = 0; i < currentDoorData.solutions.Length; i++)
        {
            if (oldNote == currentDoorData.solutions[i].correctNote)
            {
                isCorrectNote = true;

                if (currentDoorData.solutions[i].noteHeld)
                {
                    currentDoorData.solutions[i].noteHeld = false;
                    currentDoorData.solved--;
                    gemMats[i].DisableKeyword("_EMMISSION");
                    gemMats[i].SetColor("_EmissionColor", Vector4.zero);
                }
            }
        }

        if (!isCorrectNote) currentDoorData.solved++;
    }
}
