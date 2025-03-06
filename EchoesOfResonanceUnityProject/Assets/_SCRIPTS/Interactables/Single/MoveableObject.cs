using System;
using System.Collections;
using UnityEngine;

public class MoveableObject : BasicInteractable
{
    [Serializable]
    public class ObjectStep
    {
        public TrData target;
        public float shakeTime, differentDoorSpeed;
    }

    [SerializeField] private ObjectStep[] moveSteps;

    public override void ActivateObject()
    {
        base.ActivateObject();
        CRManager.root.Begin(OpenDoor(), $"{gameObject}MoveObject", this);
    }

    IEnumerator OpenDoor()
    {
        for (int i = 0; i < moveSteps.Length; i++)
        {
            if (moveSteps[i].shakeTime > 0)
            {
                 //AudioManager.root.PlaySound(AudioEvent.playDoorOpen, gameObject);

                Vector3 originalPos = transform.position;
                float elapsed = 0f;

                while (elapsed < moveSteps[i].shakeTime)
                {
                    transform.position += UnityEngine.Random.insideUnitSphere * 0.05f;
                    yield return null;

                    transform.position = originalPos;
                    elapsed += Time.deltaTime;
                }

                GameObject doorParticles = Instantiate(DH.Get<GlobalDoorData>().doorOpenParticle, transform.position - (transform.up * 2.75f), Quaternion.identity);

                Destroy(doorParticles, 5f);
            }

            CRManager.root.Restart(
                moveSteps[i].target.ApplyToOverTime(transform, (moveSteps[i].differentDoorSpeed > 0) ? moveSteps[i].differentDoorSpeed : DH.Get<GlobalDoorData>().doorMoveSpeed),
                $"{gameObject}ObjMoveStep{i}", this
            );

            yield return new WaitForSeconds((moveSteps[i].differentDoorSpeed > 0) ? moveSteps[i].differentDoorSpeed : DH.Get<GlobalDoorData>().doorMoveSpeed);
        }
    }
}

