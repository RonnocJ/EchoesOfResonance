using System.Collections;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [HideInInspector]
    public bool gemLit;
    public Material gemMat;
    private Color unlitGemColor, unlitBottomColor, unlitTopColor;
    private SkinnedMeshRenderer _mesh;
    private GlobalGemData data;
    void Awake()
    {
        _mesh = GetComponentInChildren<SkinnedMeshRenderer>();
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
        AudioManager.root.PlaySound(AudioEvent.playCheckpointReached, gameObject);
        
        AudioManager.root.SetRTPC(
            AudioRTPC.gemCheckpoint_Pitch, 
            PzUtil.GetNoteNumber(gameObject.name.Contains("#") ? gameObject.name.Substring(3, 3) : gameObject.name.Substring(3, 2)), 
            false, AudioEvent.playCheckpointReached, gameObject
        );

        transform.GetChild(1).GetComponent<ParticleSystem>().Play();
    }

    public IEnumerator ShiftGem(float newNote, float duration = 1, bool newGemLit = false)
    {
        gemLit = newGemLit;

        float elapsed = 0f;
        var newGemColor = data.gemColors[Mathf.FloorToInt(newNote / 5f)];

        while (elapsed < duration)
        {
            gemMat.SetColor("_gemColor", Vector4.Lerp(gemLit ? unlitGemColor * 200 : unlitGemColor, newGemLit ? newGemColor.mainColor * 200 : newGemColor.mainColor, elapsed / duration));
            gemMat.SetColor("_bottomColor", Vector4.Lerp(gemLit ? unlitBottomColor * 150 : unlitGemColor, newGemLit ? newGemColor.bottomColor * 150 : newGemColor.bottomColor, elapsed / duration));
            gemMat.SetColor("_topColor", Vector4.Lerp(gemLit ? unlitTopColor * 150 : unlitGemColor, newGemLit ? newGemColor.topColor * 150 : newGemColor.topColor, elapsed / duration));

            for (int j = 0; j < _mesh.sharedMesh.blendShapeCount; j++)
            {
                if (j == data.gemMeshIndicies[(int)newNote % 5])
                {
                    _mesh.SetBlendShapeWeight(j, elapsed / duration * 100f);
                }
                else if (_mesh.GetBlendShapeWeight(j) > 0)
                {
                    _mesh.SetBlendShapeWeight(j, 100f - (elapsed / duration * 100f));
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        var gemParticles = transform.GetComponentsInChildren<ParticleSystemRenderer>();

        foreach (var particle in gemParticles)
        {
            var particleMat = new Material(particle.sharedMaterial);
            particleMat.SetColor("_EmissionColor", unlitGemColor * 200f);
            particle.material = particleMat;
        }

        gemMat.SetColor("_gemColor", newGemLit ? newGemColor.mainColor * 200 : newGemColor.mainColor);
        gemMat.SetColor("_bottomColor", newGemLit ? newGemColor.bottomColor * 150 : newGemColor.bottomColor);
        gemMat.SetColor("_topColor", newGemLit ? newGemColor.topColor * 150 : newGemColor.topColor);

        unlitGemColor = newGemColor.mainColor;
        unlitBottomColor = newGemColor.bottomColor;
        unlitTopColor = newGemColor.topColor;

        for (int j = 0; j < _mesh.sharedMesh.blendShapeCount; j++)
        {
            if (j == data.gemMeshIndicies[(int)newNote % 5])
            {
                _mesh.SetBlendShapeWeight(j, 100);
            }
            else
            {
                _mesh.SetBlendShapeWeight(j, 0);
            }
        }
    }
}