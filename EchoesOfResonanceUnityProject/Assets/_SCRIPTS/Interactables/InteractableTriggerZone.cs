using System.Linq;
using UnityEngine;

public class InteractableTriggerZone : MonoBehaviour
{
    [SerializeField] private GameObject[] activateObjects;
    [SerializeField] private GameObject[] deactivateObjects;
    private bool activated;

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player") && !activated)
        {
            foreach(var a in activateObjects)
            {
                a.GetComponents<BasicInteractable>().ToList().ForEach(c => c.ActivateObject());
            }
            foreach(var d in deactivateObjects)
            {
                d.GetComponents<BasicInteractable>().ToList().ForEach(c => c.ResetObject());
            }

            activated = true;
        }
    }
}