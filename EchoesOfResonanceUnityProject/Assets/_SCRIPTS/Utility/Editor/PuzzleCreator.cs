using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PuzzleCreator : MonoBehaviour
{
    [MenuItem("Utilities/Generate New Puzzle")]
    static void CreateNewPuzzle()
    {
        PuzzleData newPuzzle = ScriptableObject.CreateInstance<PuzzleData>();
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Objects/NewPuzzleData.asset");
        AssetDatabase.CreateAsset(newPuzzle, assetPath);

        SetPositions(newPuzzle, CheckSelection());
    }

    [MenuItem("Utilities/Update Existing Puzzle")]
    static void UpdateExistingPuzzle()
    {
        PuzzleData selectedPuzzle = null;

        Selection.objects.ToList().ForEach(obj => { if (obj is PuzzleData puzzle) selectedPuzzle = puzzle; });

        if (selectedPuzzle == null)
        {
            Debug.LogError("Please select a PuzzleData ScriptableObject in the Project window!");
            return;
        }

        SetPositions(selectedPuzzle, CheckSelection());
    }

    static List<Transform> CheckSelection()
    {
        bool allGems = true;
        List<Transform> gems = new();

        Selection.transforms.ToList().ForEach(tr =>
        {
            if (tr.GetComponent<Gem>() == null) allGems = false;
            else gems.Add(tr);
        });

        if (!allGems || Selection.transforms.Length == 0)
        {
            Debug.LogError("Please only select gem objects in the Scene!");
            return null;
        }

        return gems;
    }

    static void SetPositions(PuzzleData puzzle, List<Transform> gems)
    {
        puzzle.solutions = new PuzzleData.SolutionData[gems.Count];

        for (int i = 0; i < gems.Count; i++)
        {
            PuzzleData.SolutionData solution = new PuzzleData.SolutionData();

            solution.gemTransform = new TrData
            {
                position = gems[i].localPosition,
                rotation = gems[i].rotation.eulerAngles,
                scale = gems[i].localScale
            };

            puzzle.solutions[i] = solution;
        }

        EditorUtility.SetDirty(puzzle);
        AssetDatabase.SaveAssets();
    }
    [MenuItem("Utilities/Apply Gem Color and Shape")]
    static void ApplyGemCoS()
    {
        PuzzleData selectedPuzzle = null;

        Selection.objects.ToList().ForEach(obj => { if (obj is PuzzleData puzzle) selectedPuzzle = puzzle; });

        if (selectedPuzzle == null)
        {
            Debug.LogError("Please select a PuzzleData ScriptableObject in the Project window!");
            return;
        }

        var gems = CheckSelection();

        if (gems.Count != selectedPuzzle.solutions.Length)
        {
            Debug.LogError($"Gem selection does not match solutions within the selected Scriptable Object! Please update the object first. There are {gems.Count} gems and {selectedPuzzle.solutions.Length} items in the object");
        }
        else
        {
            for (int i = 0; i < gems.Count; i++)
            {
                float newNoteFloat = PuzzleManager.root.GetNoteNumber(selectedPuzzle.solutions[i].correctNote);
                gems[i].GetChild(0).GetComponent<MeshFilter>().mesh = GlobalGemData.root.gemMeshes[((int)newNoteFloat - 1) % 5];
                gems[i].GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", GlobalGemData.root.gemColors[Mathf.FloorToInt((newNoteFloat - 1) / 5f)]);
            }
        }
    }
}