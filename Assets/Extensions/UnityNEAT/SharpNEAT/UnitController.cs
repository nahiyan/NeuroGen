using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;

public abstract class UnitController : MonoBehaviour
{
    public bool isRunning = false;
    public abstract void Activate(IBlackBox box);

    public abstract void Stop();
    public abstract float Fitness
    {
        get;
    }
}
