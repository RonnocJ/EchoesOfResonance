using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
}
#if UNITY_EDITOR 
[CustomEditor(typeof(TestOverrides))]
public class TestOverridesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Reset Save Data", GUILayout.Height(40)))
        {
            string fullPath = Path.Combine(Application.persistentDataPath, SaveDataManager.root.FileName);

            try
            {
                SaveDataManager.root.Data.Clear();

                string dataToStore = JsonConvert.SerializeObject(new Dictionary<string, object>(), Formatting.Indented);

                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not reset save data at {fullPath} \n {e}");
            }

            var puzzles = Resources.LoadAll<PuzzleData>("");

            foreach(var p in puzzles)
            {
                p.solved = 0;
            }
        }
    }
}
#endif