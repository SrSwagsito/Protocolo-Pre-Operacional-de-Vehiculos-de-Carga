using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manejador de clics/toques adaptado al nuevo Input System.
/// Soporta mouse (PC) y touch (móvil).
/// </summary>
public class ClicHandler : MonoBehaviour
{
    private Camera cam;
    public ExternalInspectionManager inspectionManager;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Detectar input tanto de mouse como de touch
        if (WasInputDetected(out Vector2 inputPosition))
        {
            ProcessClick(inputPosition);
        }
    }

    /// <summary>s
    /// Detecta si hubo input (mouse o touch) y devuelve la posición
    /// </summary>
    private bool WasInputDetected(out Vector2 position)
    {
        position = Vector2.zero;

        // Mouse (PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }

        // Touch (Móvil)
        if (Touchscreen.current != null)
        {
            for (int i = 0; i < Touchscreen.current.touches.Count; i++)
            {
                var touch = Touchscreen.current.touches[i];
                if (touch.press.wasPressedThisFrame)
                {
                    position = touch.position.ReadValue();
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Procesa el clic/touch en la posición dada
    /// </summary>
    private void ProcessClick(Vector2 screenPosition)
    {
        // Convertir posición de pantalla a mundo
        Vector3 worldPosition = cam.ScreenToWorldPoint(screenPosition);

        // Raycast 2D
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        // Verificar si golpeó algo
        if (hit.collider == null) return;

        // Buscar componente DamagedZone
        DamagedZone zone = hit.collider.GetComponent<DamagedZone>();

        if (zone != null)
        {
            zone.Report(inspectionManager);
        }
    }
}