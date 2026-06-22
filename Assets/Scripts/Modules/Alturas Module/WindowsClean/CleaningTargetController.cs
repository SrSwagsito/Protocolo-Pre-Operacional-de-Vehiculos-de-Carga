using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CleaningTargetController : MonoBehaviour
{

    public event Action<int, int> OnProgressChanged;
    public event Action OnAllTargetsCleaned;

    [SerializeField] public List<WindowSpot> spotsList;

    public int TotalTargets => spotsList.Count;

    public int CleanedCount => spotsList.Count( x => x.IsClean);
    public bool AllCleaned => spotsList.All( x => x.IsClean);

    public void RegisterCleaned(WindowSpot window)
    {

        OnProgressChanged?.Invoke(CleanedCount, TotalTargets);

        if (AllCleaned)
        {
            OnAllTargetsCleaned?.Invoke();
        }

    }
}
