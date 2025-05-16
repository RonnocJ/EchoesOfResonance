using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Gem : MonoBehaviour
{
    public bool needsLight;
    [HideInInspector]
    public bool gemLit;
    [HideInInspector]
    public bool hasLight;
    public Material gemMat;
    public PzNote gemNote;
    public Image gemIcon;
    public Action OnLightOff;
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

        OnLightOff = new Action(() => { });
    }
    public void LightOn()
    {
        gemLit = true;
        gemMat.SetColor("_gemColor", unlitGemColor * 400);
        gemMat.SetColor("_bottomColor", unlitBottomColor * 300);
        gemMat.SetColor("_topColor", unlitTopColor * 300);
        
        gemIcon.sprite = data.iconOn;
    }
    public void LightOff()
    {
        gemLit = false;
        gemMat.SetColor("_gemColor", unlitGemColor);
        gemMat.SetColor("_bottomColor", unlitBottomColor);
        gemMat.SetColor("_topColor", unlitTopColor);

        gemIcon.sprite = data.iconOff;
    }
    public void CheckpointReached()
    {
        AudioManager.root.PlaySound(AudioEvent.playCheckpointReached, gameObject);
        AudioManager.root.SetRTPC(AudioRTPC.gemCheckpoint_Pitch, gemNote.Pitch, false, AudioEvent.playCheckpointReached, gameObject);

        transform.GetChild(1).GetComponent<ParticleSystem>().Play();
    }

    public IEnumerator ShiftGem(float newNote, float duration = 1, bool newGemLit = false)
    {
        if (gemLit != newGemLit || newNote != gemNote.Pitch)
        {
            gemLit = newGemLit;

            float elapsed = 0f;
            var newGemColor = data.gemColors[Mathf.FloorToInt((newNote - 1) / 5f)];

            while (elapsed < duration)
            {
                gemMat.SetColor("_gemColor", Vector4.Lerp(gemLit ? unlitGemColor * 200 : unlitGemColor, newGemLit ? newGemColor.mainColor * 400 : newGemColor.mainColor, elapsed / duration));
                gemMat.SetColor("_bottomColor", Vector4.Lerp(gemLit ? unlitBottomColor * 150 : unlitGemColor, newGemLit ? newGemColor.bottomColor * 300 : newGemColor.bottomColor, elapsed / duration));
                gemMat.SetColor("_topColor", Vector4.Lerp(gemLit ? unlitTopColor * 150 : unlitGemColor, newGemLit ? newGemColor.topColor * 300 : newGemColor.topColor, elapsed / duration));

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

            gemMat.SetColor("_gemColor", newGemLit ? newGemColor.mainColor * 400 : newGemColor.mainColor);
            gemMat.SetColor("_bottomColor", newGemLit ? newGemColor.bottomColor * 300 : newGemColor.bottomColor);
            gemMat.SetColor("_topColor", newGemLit ? newGemColor.topColor * 300 : newGemColor.topColor);

            unlitGemColor = newGemColor.mainColor;
            unlitBottomColor = newGemColor.bottomColor;
            unlitTopColor = newGemColor.topColor;

            gemNote = new PzNote(newNote);
            if (Broadcaster.heldNotes.Contains(new PzNote(newNote)))
            {
                AudioManager.root.SetRTPC(AudioRTPC.gemHum_Pitch, newNote, false, AudioEvent.playGemHum, gameObject, 1);
            }
            else
            {
                AudioManager.root.PlaySound(AudioEvent.stopGemHum, gameObject, 1);
            }

            var gemParticles = transform.GetComponentsInChildren<ParticleSystemRenderer>();

            foreach (var particle in gemParticles)
            {
                var particleMat = new Material(particle.sharedMaterial);
                particleMat.SetColor("_EmissionColor", unlitGemColor * 200f);
                particle.material = particleMat;
            }

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

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Light") && needsLight)
        {
            hasLight = true;
        }
    }
    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Light") && needsLight)
        {
            hasLight = false;
            OnLightOff.Invoke();
        }
    }
}