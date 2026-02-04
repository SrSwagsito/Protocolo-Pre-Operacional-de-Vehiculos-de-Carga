using System;
using UnityEngine;

public class LiquidInspectionTask : MonoBehaviour
{
    public string vehiclePart;


    private LiquidInspectionManager liquidManager;
    private LiquidLevelPanel liquidPanel;

    public static Action<LiquidInspectionTask> OnTaskSelected;

    public void Initialize(LiquidInspectionManager manager, LiquidLevelPanel panel)
    {
        liquidManager = manager;
        liquidPanel = panel;

    }

    //private void OnMouseDown()
    //{
    //    levelPanel.ShowPanel();

    //    levelPanel.OnPlayerDecision = OnDecision;
    //}

    //void OnDecision(bool success)
    //{
    //    manager.OnTaskCompleted(this, success);
    //}
}
