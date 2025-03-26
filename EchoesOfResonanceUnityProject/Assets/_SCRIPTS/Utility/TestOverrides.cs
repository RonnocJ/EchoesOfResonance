using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
    public enum InputOverride
    {
        None,
        MIDI,
        CPU
    }
[CreateAssetMenu(menuName = "Objects/Utility/TestOverrides", order = 0)]
public class TestOverrides : GlobalData
{
    public bool skipIntro;
    public InputOverride inputOverride;
    [Range(0.01f, 1)]
    public float uiSpeed;
    public bool overrideSpawn;
    public Vector3 playerSpawnPosition;
}

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
        }
    }
}