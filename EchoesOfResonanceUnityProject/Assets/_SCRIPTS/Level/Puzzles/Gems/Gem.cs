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

    public IEnumerator ShiftGem(SkinnedMeshRenderer mesh, float newNote, float duration = 1, bool newGemLit = false)
    {
        float elapsed = 0f;
        var newGemColor = data.gemColors[Mathf.FloorToInt(newNote / 5f)];

        while (elapsed < duration)
        {
            gemMat.SetColor("_gemColor", Vector4.Lerp(gemLit ? unlitGemColor * 200 : unlitGemColor, newGemLit ? newGemColor.mainColor * 200 : newGemColor.mainColor, elapsed / duration));
            gemMat.SetColor("_bottomColor", Vector4.Lerp(gemLit ? unlitBottomColor * 150 : unlitGemColor, newGemLit ? newGemColor.bottomColor * 150 : newGemColor.bottomColor, elapsed / duration));
            gemMat.SetColor("_topColor", Vector4.Lerp(gemLit ? unlitTopColor * 150 : unlitGemColor, newGemLit ? newGemColor.topColor * 150 : newGemColor.topColor, elapsed / duration));

            for (int j = 0; j < mesh.sharedMesh.blendShapeCount; j++)
            {
                if (j == data.gemMeshIndicies[(int)newNote % 5])
                {
                    mesh.SetBlendShapeWeight(j, elapsed / duration * 100f);
                }
                else if (mesh.GetBlendShapeWeight(j) > 0)
                {
                    mesh.SetBlendShapeWeight(j, 100f - (elapsed / duration * 100f));
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        gemMat.SetColor("_gemColor", newGemLit ? newGemColor.mainColor * 200 : newGemColor.mainColor);
        gemMat.SetColor("_bottomColor", newGemLit ? newGemColor.bottomColor * 150 : newGemColor.bottomColor);
        gemMat.SetColor("_topColor", newGemLit ? newGemColor.topColor * 150 : newGemColor.topColor);

        unlitGemColor = newGemColor.mainColor;
        unlitBottomColor = newGemColor.bottomColor;
        unlitTopColor = newGemColor.topColor;

        for (int j = 0; j < mesh.sharedMesh.blendShapeCount; j++)
        {
            if (j == data.gemMeshIndicies[(int)newNote % 5])
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