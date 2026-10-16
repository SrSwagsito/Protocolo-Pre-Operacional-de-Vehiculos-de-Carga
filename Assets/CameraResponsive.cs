
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraResponsive : MonoBehaviour
{
    [Header("Configuración de cámara")]
    [SerializeField] private float referenceSize = 5f;

    [Tooltip("Ancho total del escenario que debe verse")]
    [SerializeField] private float worldWidth = 16f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        AjustarCamara();
    }

    private void Start()
    {
        AjustarCamara();
    }

    private void AjustarCamara()
    {
        float aspect = (float)Screen.width / Screen.height;

        float sizeNecesario = worldWidth / (2f * aspect);

        cam.orthographicSize = Mathf.Max(
            referenceSize,
            sizeNecesario
        );
    }
}
