using UnityEngine;

public class PickUpBroadcaster : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            GameManager.root.currentState = GameState.Cutscene;

            var alignPlayer = new TrData(new Vector3(0, 4, 32.9f), Quaternion.identity);
            var alignCamera = new TrData(Vector3.zero, Quaternion.Euler(15, 0, 0));

            CRManager.root.Begin(alignPlayer.ApplyToOverTime(col.transform, 0.75f), "AlignPlayerCutscene", this);
            CRManager.root.Begin(alignCamera.ApplyToOverTime(Camera.main.transform, 0.5f), "AlignCameraCutscene", this);

            col.GetComponentInChildren<ArmMover>().Invoke(nameof(ArmMover.PickUpBroadcaster), 1.5f);
        }
    }
}