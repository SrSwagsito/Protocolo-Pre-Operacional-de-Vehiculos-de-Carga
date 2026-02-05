using System;
using UnityEngine;

public class LiquidInspectionTask : MonoBehaviour
{
    public string vehiclePart;

    [SerializeField] private Collider2D taskCollider;   


    private LiquidInspectionManager liquidManager;
    private LiquidLevelPanel liquidPanel;

    public static Action<LiquidInspectionTask> OnTaskSelected;

    private void Start()
    {
        Debug.Log("Task activa " + vehiclePart);
    }

    public void Initialize(LiquidInspectionManager manager, LiquidLevelPanel panel)
    {
        liquidManager = manager;
        liquidPanel = panel;

    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clic detectado");

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D ray = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (ray.collider != null)
            {
                Debug.Log("Golpeo: " + ray.transform.name);

                LiquidInspectionTask task = ray.transform.GetComponentInParent<LiquidInspectionTask>();


                if (task == this)
                {
                    Debug.Log("Golpeo: " + ray.transform.name);
                    Debug.Log("Tarea seleccionada: " + vehiclePart);

                    OnTaskSelected?.Invoke(this);
                }
            
            }
        }
    }


        void OnDecision(bool success)
    {
        liquidManager.OnTaskCompleted(this, success);
    }
}
