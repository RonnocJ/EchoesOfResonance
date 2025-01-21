using System.Collections;
using UnityEngine;

public class DoorManager : BasicPuzzle
{
    [SerializeField] private GlobalDoorData globalDoorData;
    [SerializeField] private TrData target;

    public override void FinishedPuzzle()
    {
        base.FinishedPuzzle();
        CRManager.root.Begin(OpenDoor(), "DoorOpen", this);
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

        GameObject doorParticles = Instantiate(globalDoorData.doorOpenParticle, transform.position - (transform.up * 2.75f), Quaternion.identity);

        Destroy(doorParticles, 5f);

        CRManager.root.Begin(target.ApplyToOverTime(transform, globalDoorData.doorMoveSpeed), "DoorMove", this);
    }
}
