using UnityEngine;

public class Gem : MonoBehaviour
{
    //[HideInInspector]
    public bool gemLit;
    public Material gemMat;
    private Color unlitGemColor, unlitBottomColor, unlitTopColor;
    void Awake()
    {
        unlitGemColor = gemMat.GetColor("_gemColor");
        unlitBottomColor = gemMat.GetColor("_bottomColor");
        unlitTopColor = gemMat.GetColor("_topColor");
    }
    public void LightOn()
    {
        gemLit = true;
        gemMat.SetColor("_gemColor", unlitGemColor * 200);
        gemMat.SetColor("_bottomColor", unlitBottomColor * 150);
        gemMat.SetColor("_topColor", unlitTopColor * 150);
    }
    public void LightOff()
    {
        gemLit = false;
        gemMat.SetColor("_gemColor", unlitGemColor);
        gemMat.SetColor("_bottomColor", unlitBottomColor);
        gemMat.SetColor("_topColor", unlitTopColor);
    }
    public void CheckpointReached()
    {
        gemLit = false;
        transform.GetChild(1).GetComponent<ParticleSystem>().Play();
    }
}