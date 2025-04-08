using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSensor : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Plate") || col.CompareTag("Cutscene"))
        {
            PlayerManager.root.savedPosition = new TrData(PlayerManager.root.transform);
        }
    }
}