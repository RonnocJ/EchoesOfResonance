using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Plate"))
        {
            PlayerManager.root.savedPosition = new TrData(PlayerManager.root.transform);
        }
    }
}