using System.Linq;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;
public class PuzzleCreator : MonoBehaviour
{
    [MenuItem("Utilities/Generate New Puzzle")]
    static void CreateNewPuzzle()
    {

        Transform plate = null;

        Selection.transforms.ToList().ForEach(tr =>
        {
            if (tr.GetComponent<PuzzlePlate>() != null) plate = tr;
        });

        if (plate == null)
        {
            Debug.LogError("Puzzle Plate not selected!");
            return;
        }

        PuzzleData newPuzzle = ScriptableObject.CreateInstance<PuzzleData>();
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Objects/NewPuzzleData.asset");
        AssetDatabase.CreateAsset(newPuzzle, assetPath);

        plate.GetComponent<PuzzlePlate>().linkedData = newPuzzle;
    }

    [MenuItem("Utilities/Update Existing Puzzle")]
    static void UpdateExistingPuzzle()
    {
        Transform plate = null;

        Selection.transforms.ToList().ForEach(tr =>
        {
            if (tr.GetComponent<PuzzlePlate>() != null) plate = tr;
        });

        if (plate == null)
        {
            Debug.LogError("Puzzle Plate not selected!");
            return;
        }

        PuzzleData data = null;
        Selection.objects.ToList().ForEach(obj => { if (obj is PuzzleData puzzle) data = puzzle; });

        if (data == null)
        {
            Debug.LogError("Puzzle Scriptable Object not selected!");
            return;
        }

        plate.GetComponent<PuzzlePlate>().linkedData = data;

        var gems = plate.GetComponentsInChildren<Gem>();

        if (gems == null || gems.Length != data.solutions.Length)
        {
            Debug.LogError($"Mistmatch between gem objects and expected gems. The plate has {gems.Length} gems as children, but was expecting {data.solutions.Length} gems");
            return;
        }

        for (int i = 0; i < gems.Length; i++)
        {
            float newNoteFloat = PuzzleUtilities.root.GetNoteNumber(data.solutions[i]);
            gems[i].transform.GetChild(0).GetComponent<MeshFilter>().mesh = DH.Get<GlobalGemData>().gemMeshes[((int)newNoteFloat - 1) % 5];

            var gemMat = new Material(gems[i].transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial);
            gems[i].transform.GetChild(0).GetComponent<MeshRenderer>().material = gemMat;

            gemMat.SetColor("_gemColor", DH.Get<GlobalGemData>().gemColors[Mathf.FloorToInt((newNoteFloat - 1) / 5f)]);
            gems[i].gemMat = gemMat;

            if (PrefabUtility.IsPartOfAnyPrefab(gems[i].gameObject))
                PrefabUtility.UnpackPrefabInstance(gems[i].gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }

        plate.GetComponent<PuzzlePlate>().gems = gems;

        var puzzles = plate.GetComponentsInChildren<BasicPuzzle>();

        if (puzzles != null)
        {
            plate.GetComponent<PuzzlePlate>().linkedPuzzles = puzzles;

            foreach (var p in puzzles)
            {
                p.linkedData = data;

                if (PrefabUtility.IsPartOfAnyPrefab(p.gameObject))
                    PrefabUtility.UnpackPrefabInstance(p.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);

                switch (p)
                {
                    case DoorManager door:

                        break;

                    case TorchManager torch:

                        torch.flameParticle = torch.transform.GetChild(0).GetComponent<ParticleSystem>();
                        torch.glowParticle = torch.transform.GetChild(1).GetComponent<ParticleSystem>();

                        torch.torchLight = torch.transform.GetChild(2).GetComponent<Light>();
                        torch.torchLight.enabled = false;

                        break;
                }
            }
        }

        if (PrefabUtility.IsPartOfAnyPrefab(plate.gameObject))
            PrefabUtility.UnpackPrefabInstance(plate.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);

        Debug.Log("Puzzle updated successfully!");
    }
}