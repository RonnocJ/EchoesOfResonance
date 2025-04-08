using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/Global/GlobalTorchData", order = 3)]
public class GlobalTorchData : GlobalData
{
    public AK.Wwise.Event torchLightUp, torchExtinguish;
}
