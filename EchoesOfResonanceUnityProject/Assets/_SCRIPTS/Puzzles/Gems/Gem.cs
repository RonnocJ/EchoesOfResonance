using System.Collections;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [HideInInspector]
    public bool gemLit;
    public Material gemMat;
    private Color unlitGemColor, unlitBottomColor, unlitTopColor;
    private GlobalGemData data;
    void Awake()
    {
        data = DH.Get<GlobalGemData>();

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
        AudioManager.root.PlaySound(AudioEvent.playBroadcasterPlunk, gameObject);
        transform.GetChild(1).GetComponent<ParticleSystem>().Play();
    }

    public IEnumerator ShiftGem(SkinnedMeshRenderer mesh, int meshIndex, float duration = 1)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            for (int j = 0; j < mesh.sharedMesh.blendShapeCount; j++)
            {
                if (j == data.gemMeshIndicies[meshIndex])
                {   
                    mesh.SetBlendShapeWeight(j, elapsed / duration * 100f);
                }
                else if(mesh.GetBlendShapeWeight(j) > 0)
                {
                    mesh.SetBlendShapeWeight(j, 100f - (elapsed / duration * 100f));
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int j = 0; j < mesh.sharedMesh.blendShapeCount; j++)
            {
                if (j == data.gemMeshIndicies[meshIndex])
                {   
                    mesh.SetBlendShapeWeight(j, 100);
                }
                else 
                {
                    mesh.SetBlendShapeWeight(j, 0);
                }
            }
    }
}