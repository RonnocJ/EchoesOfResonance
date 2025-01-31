using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Objects/Puzzles/Global/GlobalPlateData", order = 1)]
public class GlobalPlateData : GlobalData
{
    public float alignSpeed;
    public float ejectForce;
    public Color activeColor;
    public Color completedColor;

}
