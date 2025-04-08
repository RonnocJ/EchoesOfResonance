using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
public class SaveDataManager : Singleton<SaveDataManager>
{
    public Dictionary<string, object> Data = new();
    public string FileName;
    void Start()
    {
        ReadAllData();
    }
    void ReadAllData()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, FileName);

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

        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var b in allBehaviours)
        {
            if (b.TryGetComponent(out ISaveData saveData))
            {
                if (Data.ContainsKey(b.gameObject.name) && Data[b.gameObject.name] is Newtonsoft.Json.Linq.JObject jObject)
                    saveData.ReadSaveData(jObject.ToObject<Dictionary<string, object>>());
                else
                {
                    saveData.ReadSaveData(new Dictionary<string, object>());
                }
            }
        }
    }
    void WriteAllData()
    {
        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var b in allBehaviours)
        {
            if (b.TryGetComponent(out ISaveData saveData))
            {
                string objectKey = b.gameObject.name;
                Data[objectKey] = saveData.AddSaveData();
            }
        }

        string fullPath = Path.Combine(Application.persistentDataPath, FileName);

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

    void OnApplicationQuit()
    {
        if (DH.Get<TestOverrides>().saveProgress)
            WriteAllData();
    }
}