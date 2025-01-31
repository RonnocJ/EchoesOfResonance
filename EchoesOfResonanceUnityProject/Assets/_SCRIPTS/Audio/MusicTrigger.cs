using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    public AK.Wwise.Event[] events;
    public AK.Wwise.State[] states;

    void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            foreach(var e in events)
            {
                e.Post(transform.parent.gameObject);
            }

            foreach(var s in states)
            {
                s.SetValue();
            }
        }
    }
}
