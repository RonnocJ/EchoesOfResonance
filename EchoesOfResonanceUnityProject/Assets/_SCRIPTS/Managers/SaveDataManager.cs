using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

[Serializable]
public class SaveDataProfile
{
    public string FilePath;
    public string FileName;
    public Dictionary<string, object> Data;
    public SaveDataProfile(string filePath, string fileName)
    {
        FilePath = filePath;
        FileName = fileName;
        Data = new();
    }
    public void ReadAllData(MonoBehaviour[] allBehaviours)
    {
        Data = new();

        string fullPath = Path.Combine(FilePath, FileName);

        if (File.Exists(fullPath))
        {
            try
            {
                string loadedData = File.ReadAllText(fullPath);
                Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(loadedData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error trying to load save data from path {fullPath} \n {e}");
            }
        }

        int i = 0;

        foreach (var b in allBehaviours)
        {
            if (b is ISaveData saveData)
            {
                i++;

                if (Data.ContainsKey(GetStableID(b)) && Data[GetStableID(b)] is Newtonsoft.Json.Linq.JObject jObject)
                {
                    saveData.ReadSaveData(jObject.ToObject<Dictionary<string, object>>());
                }

                else
                    saveData.ReadSaveData(new Dictionary<string, object>());
            }
        }
    }
    public void WriteAllData(MonoBehaviour[] allBehaviours)
    {
        Data = new();

        int i = 0;

        foreach (var b in allBehaviours)
        {
            if (b is ISaveData saveData)
            {
                i++;

                if (saveData.AddSaveData() != null)
                    Data[GetStableID(b)] = saveData.AddSaveData();
            }
        }

        string fullPath = Path.Combine(FilePath, FileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonConvert.SerializeObject(Data, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not save file to {fullPath} \n {e}");
        }
    }
    public void ResetData()
    {
        string fullPath = Path.Combine(FilePath, FileName);

        try
        {
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

    private string GetStableID(MonoBehaviour behaviour)
    {
        var idHolder = behaviour.GetComponent<ObjectIDManager>();
        if (idHolder == null)
        {
            #if UNITY_EDITOR
                idHolder = UnityEditor.Undo.AddComponent<ObjectIDManager>(behaviour.gameObject);
                UnityEditor.EditorUtility.SetDirty(behaviour.gameObject);
            #endif
        }

        return idHolder?.ID;
    }
}
public class SaveDataManager : Singleton<SaveDataManager>
{
    [SerializeField] private string _fileName;
    public SaveDataProfile MainProfile
    {
        get => new SaveDataProfile(Application.persistentDataPath, _fileName);
    }
    public Action<Dictionary<string, object>> LoadNewData;
    void Start()
    {
        DontDestroyOnLoad(this);
        MainProfile.ReadAllData(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID));
    }
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        
        if (DH.Get<TestOverrides>().saveProgress)
            MainProfile.WriteAllData(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID));
    }
}
#if UNITY_EDITOR

[InitializeOnLoad]
public static class SaveDataIDAutoAssigner
{
    static SaveDataIDAutoAssigner()
    {
        EditorApplication.delayCall += AssignUniqueIDsToAllSaveData;
    }

    private static void AssignUniqueIDsToAllSaveData()
    {
        ISaveData[] saveDataComponents = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISaveData>()
            .ToArray();

        foreach (var saveData in saveDataComponents)
        {
            var behaviour = (MonoBehaviour)saveData;
            if (behaviour.GetComponent<ObjectIDManager>() == null)
            {
                Undo.AddComponent<ObjectIDManager>(behaviour.gameObject);
                EditorUtility.SetDirty(behaviour.gameObject);
            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
