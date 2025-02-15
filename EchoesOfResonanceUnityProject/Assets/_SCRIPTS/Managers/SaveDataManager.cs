using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
public class SaveDataManager : Singleton<SaveDataManager>
{
    public Dictionary<string, object> data = new();
    [SerializeField] private string fileName;
    protected override void Awake()
    {
        base.Awake();
        ReadAllData();
    }
    void ReadAllData()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(fullPath))
        {
            try
            {
                string loadedData = File.ReadAllText(fullPath);
                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(loadedData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error trying to load save data from path {fullPath} \n {e}");
            }
        }

        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var b in allBehaviours)
        {    
            if (b.TryGetComponent(out ISaveData saveData) && data[b.gameObject.name] is Newtonsoft.Json.Linq.JObject jObject)
            {
               saveData.ReadSaveData(jObject.ToObject<Dictionary<string, object>>());
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
                data[objectKey] = saveData.AddSaveData();
            }
        }

        string fullPath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonConvert.SerializeObject(data, Formatting.Indented);

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
        WriteAllData();
    }
}