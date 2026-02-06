using System;
using UnityEngine;
using UnityEngine.EventSystems; // NUEVO: Para detectar clics sobre UI

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
            // NUEVO: Verificar si el clic está sobre un elemento de UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // Si el clic es sobre UI, ignorarlo
            }

            // NUEVO: Verificar si el panel está abierto
            if (liquidPanel != null && liquidPanel.gameObject.activeSelf)
            {
                return; // Si el panel está abierto, ignorar clics en las tareas
            }

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