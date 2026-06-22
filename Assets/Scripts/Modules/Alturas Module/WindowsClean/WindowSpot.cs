using System.Collections;
using UnityEngine;

public class WindowSpot : MonoBehaviour, ICleanable
{
    [SerializeField] CleaningTargetController controller;

    private bool cleaned = false;
    public bool IsClean => cleaned;

    public void Clean()
    {
        if (cleaned) return;

        cleaned = true;

        controller.RegisterCleaned(this);
    }
}
