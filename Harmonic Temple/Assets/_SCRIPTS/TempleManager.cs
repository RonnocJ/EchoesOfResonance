using System.Collections;
using UnityEngine;

public class TempleManager : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject roomModule;
    void Start()
    {
        for(int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            Instantiate(roomModule, Vector3.forward * i * 12, Quaternion.identity, transform);
        }
    }

    public IEnumerator MoveForward()
    {
        yield return new WaitForSeconds(1f);

        float startZ = transform.position.z;
        while(transform.position.z > startZ - 12)
        {
            transform.position -= Vector3.forward * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
