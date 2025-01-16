using UnityEngine;

public class Gem : MonoBehaviour
{
    private Material gemMat;
    void Awake()
    {
        gemMat = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        gemMat.SetColor("_EmissionColor", Vector4.zero);
    }
    public void LightOn()
    {
        gemMat.SetColor("_EmissionColor", gemMat.color * 50);
    }
    public void LightOff()
    {
        gemMat.SetColor("_EmissionColor", Vector4.zero);
    }
}