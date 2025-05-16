using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
[Serializable]
public class SaveDataEntry
{
    public string name;
    public SaveDataProfile profile;
    public SaveDataEntry(string newName, SaveDataProfile newProfile)
    {
        name = newName;
        profile = newProfile;
    }
}
#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Objects/Utility/TestOverrides", order = 0)]
#endif
public class TestOverrides : GlobalData
{
    public bool skipIntro;
    public bool saveProgress;
    [Range(0.01f, 1)]
    public float uiSpeed;
    public bool overrideSpawn;
    public Vector3 playerSpawnPosition;
    public List<SaveDataEntry> SavedProfiles;
}
#if UNITY_EDITOR 
[CustomEditor(typeof(TestOverrides))]
public class TestOverridesEditor : Editor
{
    private TestOverrides testOverrides;

    private void OnEnable()
    {
        testOverrides = (TestOverrides)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Save Data Profile Management", EditorStyles.boldLabel);

        for (int i = 0; i < testOverrides.SavedProfiles.Count; i++)
        {
            var entry = testOverrides.SavedProfiles[i];
            EditorGUILayout.BeginVertical(GUI.skin.box);
            entry.name = EditorGUILayout.TextField("Profile Name", entry.name);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save") && Application.isPlaying)
            {
                SaveEntry(entry);
            }

            if (GUILayout.Button("Load") && Application.isPlaying)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                SceneManager.sceneLoaded += (_, _) => entry.profile?.ReadAllData(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID));
            }

            if (GUILayout.Button("Delete"))
            {
                if (entry.profile != null)
                {
                    string path = Path.Combine(entry.profile.FilePath, entry.profile.FileName);

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }

                testOverrides.SavedProfiles.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add New Profile Entry"))
        {
            var newEntry = new SaveDataEntry($"NewProfile_{testOverrides.SavedProfiles.Count + 1}", new SaveDataProfile(Application.streamingAssetsPath + "/TestProfiles", $"NewProfile_{testOverrides.SavedProfiles.Count + 1}.json"));

            testOverrides.SavedProfiles.Add(newEntry);

            if (Application.isPlaying)
                SaveEntry(newEntry);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Reset Save Data", GUILayout.Height(40)))
        {
            SaveDataManager.root.MainProfile.ResetData();

            var puzzles = Resources.LoadAll<PuzzleData>("");
            foreach (var p in puzzles)
            {
                p.solved = 0;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    void SaveEntry(SaveDataEntry entry)
    {
        string path = Path.Combine(entry.profile.FilePath, entry.profile.FileName);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        entry.profile.FileName = entry.name + ".json";
        entry.profile?.WriteAllData(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID));

        EditorUtility.SetDirty(testOverrides);
        AssetDatabase.SaveAssets();
    }
}

#endif