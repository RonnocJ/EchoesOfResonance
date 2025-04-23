using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSensor : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Plate") || col.CompareTag("Cutscene"))
        {
            PlrMngr.root.savedPosition = new TrData(PlrMngr.root.transform);
        }
    }
}