using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoadingManager : Singleton<LoadingManager>
{
    public List<Task> LoadingTasks = new();
    protected async override void Awake()
    {
        base.Awake();

        foreach(var t in LoadingTasks)
        {
            await t;
        }
    }
}