using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class DoorManager : BasicPuzzle
{
    [SerializeField] private GlobalDoorData globalDoorData;
    private Coroutine doorRoutine;

    public override void FinishedPuzzle()
    {
        base.FinishedPuzzle();
        if (doorRoutine == null)
        {
            doorRoutine = StartCoroutine(OpenDoor());
        }
    }

    IEnumerator OpenDoor()
    {
        globalDoorData.doorOpening.Post(gameObject);

        Vector3 originalPos = transform.position;

        for (int i = 0; i < 60; i++)
        {
            transform.position += Random.insideUnitSphere * 0.05f;
            yield return null;
            transform.position = originalPos;
        }

        GameObject doorParticles = Instantiate(globalDoorData.doorOpenParticle, transform.position - (Vector3.up * 2.75f), Quaternion.identity);
        Destroy(doorParticles, 5f);

        while (transform.localPosition.y < 7.5f)
        {
            transform.position += Vector3.up * Time.deltaTime * globalDoorData.doorMoveSpeed;
            yield return null;
        }
    }
}
