using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class FakeBroadcaster : MonoBehaviour, ISaveData
{
        public Dictionary<string, object> AddSaveData()
    {
        return new Dictionary<string, object>
        {
            {"lastPosition", new SaveStruct(new TrData(transform, TrData.IncludeInMove.Position | TrData.IncludeInMove.Rotation | TrData.IncludeInMove.Scale))}
        };
    }
    public void ReadSaveData(Dictionary<string, object> savedData)
    {
        if (savedData.TryGetValue("lastPosition", out object lastPosRaw))
        {
            string json = JsonConvert.SerializeObject(lastPosRaw);
            SaveStruct lastPos = JsonConvert.DeserializeObject<SaveStruct>(json);

            TrData lastPosition = lastPos.LoadData();
            lastPosition.ApplyTo(transform);
        }
    }
}