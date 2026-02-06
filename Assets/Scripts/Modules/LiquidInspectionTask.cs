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


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);


            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);


            if (hit.collider != null)
            {
                LiquidInspectionTask task = hit.transform.GetComponentInParent<LiquidInspectionTask>();


                if (task == this)
                {
                    OnTaskSelected?.Invoke(this);
                }
            }
        }
    }
}