using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public class DirectionalObject : BasicInteractable
{
    [SerializeField] private Vector3 moveDir;
    [SerializeField] private float moveSpeed;
    private Collider col;
    public override void Awake()
    {
        base.Awake();
        col = GetComponent<Collider>();
    }
    public override void ActivateObject()
    {
        base.ActivateObject();

        CRManager.Restart(ConstantMove(), $"{gameObject}MoveDirection", this);
    }
    IEnumerator ConstantMove()
    {
        while (true)
        {
            RaycastHit[] hits = 
                Physics.BoxCastAll(
                    col.bounds.center, 
                    col.bounds.extents, moveDir, Quaternion.identity, (moveSpeed * Time.deltaTime) + 2, 1 << 6);
            
            foreach(var h in hits)
            {
                if(h.collider != col)
                {
                    ResetObject();
                    yield break;
                }
            }

            transform.position += moveDir * moveSpeed * Time.deltaTime;

            yield return null;
        }
    }
    public override void ResetObject()
    {
        base.ResetObject();

        CRManager.Stop($"{gameObject}MoveDirection", this);
    }
}