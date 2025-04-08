using System.Linq;
using UnityEditor;
using UnityEngine;

public class PuzzleCreator : MonoBehaviour
{
    private static Transform plate;
    private static PuzzleData data;
    [MenuItem("Utilities/Generate New Puzzle")]
    static void CreateNewPuzzle()
    {
        plate = null;

        plate = GetPlate();

        if (plate == null)
        {
            Debug.LogError("Puzzle Plate not selected!");
            return;
        }

        PuzzleData newPuzzle = ScriptableObject.CreateInstance<PuzzleData>();
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Objects/{plate.name}.asset");
        AssetDatabase.CreateAsset(newPuzzle, assetPath);

        plate.GetComponent<PuzzlePlate>().linkedData = newPuzzle;
    }

    [MenuItem("Utilities/Update Immoveable Puzzle")]
    static void UpdateImmoveableExistingPuzzle()
    {
        plate = null;
        data = null;

        plate = GetPlate();

        if (plate == null)
        {
            Debug.LogError("Puzzle Plate not selected!");
            return;
        }

        data = GetData();

        if (data == null)
        {
            Debug.LogError("Puzzle Scriptable Object not selected!");
            return;
        }

        SetGems(plate);
        SetInteractables();

        UnpackPrefab(plate.gameObject);

        Debug.Log("Puzzle updated successfully!");
    }
    [MenuItem("Utilities/Update Moveable Puzzle")]
    static void UpdateMoveableExistingPuzzle()
    {
        plate = null;
        data = null;

        plate = GetPlate();

        if (plate == null)
        {
            Debug.LogError("Puzzle Plate not selected!");
            return;
        }

        data = GetData();

        if (data == null)
        {
            Debug.LogError("Puzzle Scriptable Object not selected!");
            return;
        }

        Selection.transforms.ToList().ForEach(tr =>
        {
            if (tr.GetComponentsInChildren<Gem>().Length > 0) SetGems(tr);
        });

        SetInteractables();

        UnpackPrefab(plate.gameObject);

        Debug.Log("Puzzle updated successfully!");
    }

    static Transform GetPlate()
    {
        Transform plate = null;

        Selection.transforms.ToList().ForEach(tr =>
        {
            if (tr.GetComponent<PuzzlePlate>() != null) plate = tr;
        });

        return plate;
    }

    static PuzzleData GetData()
    {
        PuzzleData data = null;
        Selection.objects.ToList().ForEach(obj => { if (obj is PuzzleData puzzle) data = puzzle; });
        plate.GetComponent<PuzzlePlate>().linkedData = data;
        return data;
    }

    static void SetGems(Transform gemParent)
    {
        var gems = gemParent.GetComponentsInChildren<Gem>();

        if (gems == null || gems.Length != data.solutions.Length)
        {
            Debug.LogError($"Mistmatch between gem objects and expected gems. The plate has {gems.Length} gems as children, but was expecting {data.solutions.Length} gems");
            return;
        }

        for (int i = 0; i < gems.Length; i++)
        {
            float newNoteFloat = PuzzleUtilities.root.GetNoteNumber(data.solutions[i].noteName);
            var gemMesh = gems[i].transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();

            for (int j = 0; j < gemMesh.sharedMesh.blendShapeCount; j++)
            {
                gemMesh.SetBlendShapeWeight(j, (j == DH.Get<GlobalGemData>().gemMeshIndicies[(int)newNoteFloat % 5]) ? 100f : 0f);
            }

            var gemMat = new Material(gems[i].transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMaterial);
            gems[i].transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = gemMat;

            gemMat.SetColor("_gemColor", DH.Get<GlobalGemData>().gemColors[Mathf.FloorToInt((newNoteFloat - 1) / 5f)].mainColor);
            gemMat.SetColor("_bottomColor", DH.Get<GlobalGemData>().gemColors[Mathf.FloorToInt((newNoteFloat - 1) / 5f)].bottomColor);
            gemMat.SetColor("_topColor", DH.Get<GlobalGemData>().gemColors[Mathf.FloorToInt((newNoteFloat - 1) / 5f)].topColor);
            gems[i].gemMat = gemMat;

            var gemParticles = gems[i].transform.GetComponentsInChildren<ParticleSystemRenderer>();

            foreach (var particle in gemParticles)
            {
                var particleMat = new Material(particle.sharedMaterial);
                particleMat.SetColor("_EmissionColor", DH.Get<GlobalGemData>().gemColors[Mathf.FloorToInt((newNoteFloat - 1) / 5f)].mainColor * 200f);
                particle.material = particleMat;
            }

            if (PrefabUtility.IsPartOfAnyPrefab(gems[i].gameObject))
                PrefabUtility.UnpackPrefabInstance(gems[i].gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);

            gems[i].gameObject.name = $"Gem{data.solutions[i].noteName}_{i}";
        }

        plate.GetComponent<PuzzlePlate>().gems = gems;
    }

    static void SetInteractables()
    {
        var interactables = plate.GetComponentsInChildren<BasicInteractable>();

        if (interactables != null)
        {
            foreach (var i in interactables)
            {
                if (i.IsLinkedWithPuzzle) i.LinkedData = data;
                UnpackPrefab(i.gameObject);

                switch (i)
                {
                    case MoveableObject door:

                        break;

                    case Torch torch:

                        torch.flameParticle = torch.transform.GetChild(0).GetComponent<ParticleSystem>();
                        torch.glowParticle = torch.transform.GetChild(1).GetComponent<ParticleSystem>();

                        torch.torchLight = torch.transform.GetChild(2).GetComponent<Light>();
                        torch.torchLight.enabled = false;

                        break;
                }
            }
        }
    }
    static void UnpackPrefab(GameObject obj)
    {
        if (PrefabUtility.IsPartOfAnyPrefab(obj))
            PrefabUtility.UnpackPrefabInstance(obj.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
    }
}