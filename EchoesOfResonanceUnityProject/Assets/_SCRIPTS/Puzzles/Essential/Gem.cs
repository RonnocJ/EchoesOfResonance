using UnityEngine;

public class Gem : MonoBehaviour
{
    [HideInInspector]
    public bool gemLit;
    public Material gemMat;
    void Awake()
    {
        gemMat.SetColor("_glowColor", Vector4.zero);
    }
    public void LightOn()
    {
        gemLit = true;
        gemMat.SetColor("_glowColor", gemMat.GetColor("_gemColor") * 5);
    }
    public void LightOff()
    {
        gemLit = false;
        gemMat.SetColor("_glowColor", Vector4.zero);
    }
}