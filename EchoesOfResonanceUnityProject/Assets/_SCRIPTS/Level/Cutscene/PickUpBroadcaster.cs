using System.Collections.Generic;
using UnityEngine;

public class PickUpBroadcaster : MonoBehaviour
{
    [SerializeField] private Vector3 _alignPosition, _alignView;
    private bool _triggeredCutscene;
    void Start()
    {
        _triggeredCutscene = ArmMover.HasBroadcaster;
    }
    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player") && !_triggeredCutscene)
        {
            GameManager.root.State = GameState.Cutscene;

            var alignPlayer = new TrData(_alignPosition, Quaternion.identity);
            var alignCamera = new TrData(Vector3.zero, Quaternion.Euler(_alignView));

            CRManager.Begin(alignPlayer.ApplyToOverTime(col.transform, 1f), "AlignPlayerCutscene", this);
            CRManager.Begin(alignCamera.ApplyToOverTime(Camera.main.transform, 0.75f), "AlignCameraCutscene", this);

            col.GetComponentInChildren<ArmMover>().Invoke(nameof(ArmMover.PickUpBroadcaster), 1.5f);

            PlrMngr.root.moveInput = 0;

            MusicManager.root.SetState(AudioState.Opening01_BREAK_GetBroadcaster);
            MusicManager.root.currentSong.AddQueuedCallback("EndOpeningMusic", 28, () => MusicManager.root.SetState(AudioState.Opening01_BREAK_None));
            
            _triggeredCutscene = true;
        }
    }
}