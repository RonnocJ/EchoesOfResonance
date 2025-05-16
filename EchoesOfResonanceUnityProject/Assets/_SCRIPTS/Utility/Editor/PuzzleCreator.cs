using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleCreator : MonoBehaviour
{
    private static PuzzleStand plate;
    private static PuzzleData data;
    [MenuItem("Utilities/Generate New Puzzle")]
    static void CreateNewPuzzle()
    {
        DH.RegisterAllAtStartup();

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
    }

    [MenuItem("Utilities/Update Immoveable Puzzle/Gems in Order")]
    static void UpdateOrdImExPuzzle()
    {
        DH.RegisterAllAtStartup();

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

        SetGems();
        SetStandUI();
        SetInteractables();

        UnpackPrefab(plate.gameObject);

        Debug.Log("Puzzle updated successfully!");
    }
    [MenuItem("Utilities/Update Immoveable Puzzle/Gems out of Order")]
    static void UpdateDisImExPuzzle()
    {
        DH.RegisterAllAtStartup();

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

        SetGems(false);
        SetStandUI();
        SetInteractables();

        UnpackPrefab(plate.gameObject);

        Debug.Log("Puzzle updated successfully!");
    }
    [MenuItem("Utilities/Update Moveable Puzzle")]
    static void UpdateMoveableExistingPuzzle()
    {
        DH.RegisterAllAtStartup();

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

        Selection.objects.OfType<GameObject>().Select(go => go.transform).ToList().ForEach(tr =>
            {
                if (tr.GetComponentsInChildren<Gem>().Length > 0) SetGems(tr);
            }
        );

        SetInteractables();

        UnpackPrefab(plate.gameObject);

        Debug.Log("Puzzle updated successfully!");
    }

    static PuzzleStand GetPlate()
    {
        PuzzleStand plate = null;

        Selection.transforms.ToList().ForEach(tr =>
        {
            if (tr.TryGetComponent(out PuzzleStand newPlate)) plate = newPlate;
        });

        return plate;
    }

    static PuzzleData GetData()
    {
        PuzzleData data = null;
        Selection.objects.ToList().ForEach(obj => { if (obj is PuzzleData puzzle) data = puzzle; });
        return data;
    }

    static void SetGems(bool inOrder = true)
    {
        var g = plate.GetComponentsInChildren<Gem>();
        IEnumerable<Gem> gems = g;
        var gData = DH.Get<GlobalGemData>();

        if (gems == null || gems.Count() != data.solutions.Length)
        {
            Debug.LogError($"Mistmatch between gem objects and expected gems. The plate has {gems.Count()} gems as children, but was expecting {data.solutions.Length} gems");
            return;
        }

        if (!inOrder) gems = gems.OrderBy(c =>
        {
            var match = Regex.Match(c.gameObject.name, @"\d+");
            return match.Success ? int.Parse(match.Value) : int.MaxValue;
        });

        for (int j = 0; j < gems.Count(); j++)
        {
            gems.ElementAt(j).gemNote = data.solutions[j].note;
            int newNote = (int)gems.ElementAt(j).gemNote.Pitch;

            var gemMesh = gems.ElementAt(j).transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();

            for (int k = 0; k < gemMesh.sharedMesh.blendShapeCount; k++)
            {
                gemMesh.SetBlendShapeWeight(k, (k == gData.gemMeshIndicies[newNote % 5]) ? 100f : 0f);
            }

            var gemMat = new Material(gems.ElementAt(j).transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMaterial);
            gems.ElementAt(j).transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = gemMat;

            gemMat.SetColor("_gemColor", gData.gemColors[Mathf.FloorToInt((newNote - 1) / 5f)].mainColor);
            gemMat.SetColor("_bottomColor", gData.gemColors[Mathf.FloorToInt((newNote - 1) / 5f)].bottomColor);
            gemMat.SetColor("_topColor", gData.gemColors[Mathf.FloorToInt((newNote - 1) / 5f)].topColor);
            gems.ElementAt(j).gemMat = gemMat;

            var gemParticles = gems.ElementAt(j).transform.GetComponentsInChildren<ParticleSystemRenderer>();

            foreach (var particle in gemParticles)
            {
                var particleMat = new Material(particle.sharedMaterial);
                particleMat.SetColor("_EmissionColor", gData.gemColors[Mathf.FloorToInt((newNote - 1) / 5f)].mainColor * 200f);
                particle.material = particleMat;
            }

            if (PrefabUtility.IsPartOfAnyPrefab(gems.ElementAt(j).gameObject))
                PrefabUtility.UnpackPrefabInstance(gems.ElementAt(j).gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);

            if (inOrder) gems.ElementAt(j).gameObject.name = $"Gem_{j}";
        }

        plate.gems = gems.ToArray();
    }
    static void SetStandUI()
    {
        var gData = DH.Get<GlobalGemData>();
        plate.standTr = plate.transform.Find("Stand");
        Transform standUI = plate.GetComponentInChildren<Canvas>().transform.GetChild(0);

        if (standUI == null || standUI.childCount < 1)
        {
            Debug.LogError("Please add the proper plate UI element!");
            return;
        }

        for (int i = standUI.childCount - 1; i >= 1; i--)
        {
            DestroyImmediate(standUI.GetChild(i).gameObject);
        }

        plate.progressText = standUI.GetChild(0).GetComponent<TextMeshProUGUI>();

        List<Transform> uiElements = new();
        int gemCount = 0;
        int checkpointCount = 0;

        foreach (var s in data.solutions)
        {
            if (s.checkpoint)
            {
                GameObject newCheckpoint = Instantiate(gData.checkpointPrefab);
                uiElements.Add(newCheckpoint.transform);

                newCheckpoint.transform.SetParent(standUI);

                newCheckpoint.name = $"CheckpointIcon_{checkpointCount}";
                checkpointCount++;
            }

            GameObject newIcon = Instantiate(gData.iconPrefab);
            uiElements.Add(newIcon.transform);

            newIcon.transform.SetParent(standUI);

            newIcon.name = $"GemIcon_{gemCount}";
            
            plate.gems[gemCount].gemIcon = newIcon.GetComponent<Image>();

            gemCount++;
        }

        float spacing = 0.185f;
        float maxPerRow = Mathf.Floor(2.7f / spacing);

        for (int j = 0; j < uiElements.Count; j++)
        {
            int row = j / (int)maxPerRow;
            int col = j % (int)maxPerRow;

            int itemsInThisRow = Mathf.Min(uiElements.Count - row * (int)maxPerRow, (int)maxPerRow);
            float rowOffset = (itemsInThisRow - 1) * spacing * 0.5f;

            float x = (col * spacing) - rowOffset;
            float y = -row / 2f;

            uiElements[j].localPosition = new Vector3(x, y, 0f);
            uiElements[j].localEulerAngles = Vector3.zero;
        }
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