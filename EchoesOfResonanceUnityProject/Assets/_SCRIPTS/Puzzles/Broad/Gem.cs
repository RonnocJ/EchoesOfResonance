using UnityEngine;

public class Gem : MonoBehaviour
{
    [HideInInspector]
public bool gemLit;
    private Material gemMat;
    void Awake()
    {
        gemMat = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        gemMat.SetColor("_EmissionColor", Vector4.zero);
    }
    public void LightOn()
    {
        gemLit = true;
        gemMat.SetColor("_EmissionColor", gemMat.color * 50);
    }
    public void LightOff()
    {
        gemLit = false;
        gemMat.SetColor("_EmissionColor", Vector4.zero);
    }
}