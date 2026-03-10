using UnityEngine;

/// <summary>
/// Parallax mejorado con control de activación/desactivación.
/// Permite pausar el movimiento cuando el juego no ha iniciado.
/// </summary>
public class ParallaxMovement : MonoBehaviour
{
    [SerializeField] private Vector2 movementSpeed;

    [Header("Control")]
    [Tooltip("Si está desmarcado, el parallax no se moverá")]
    [SerializeField] private bool isActive = false; // Empieza pausado

    private Vector2 offset;
    private Material material;
    private Vector2 savedOffset; // Para guardar el estado

    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
        savedOffset = material.mainTextureOffset;
    }

    private void Update()
    {
        // Solo mover si está activo
        if (!isActive) return;

        offset = movementSpeed * Time.deltaTime;
        material.mainTextureOffset += offset;
    }

    /// <summary>
    /// Activa o desactiva el movimiento del parallax
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
    }

    /// <summary>
    /// Resetea el offset al inicio
    /// </summary>
    public void ResetOffset()
    {
        if (material != null)
        {
            material.mainTextureOffset = Vector2.zero;
        }
    }

    /// <summary>
    /// Pausa el movimiento guardando el estado actual
    /// </summary>
    public void Pause()
    {
        savedOffset = material.mainTextureOffset;
        isActive = false;
    }

    /// <summary>
    /// Reanuda el movimiento desde donde se pausó
    /// </summary>
    public void Resume()
    {
        if (material != null)
        {
            material.mainTextureOffset = savedOffset;
        }
        isActive = true;
    }
}