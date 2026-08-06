using UnityEngine;
using System;

/// <summary>
/// Script de cada obstáculo que cae durante la Fase 2.
///
/// Cambios respecto a la versión anterior:
/// - En lugar de Destroy(), llama _returnCallback para devolverse al pool.
/// - Se recicla automáticamente cuando su posición Y cae por debajo de recycleY.
/// - Initialize() se llama desde ObstacleSpawner cada vez que el objeto
///   se activa desde el pool.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FallingObstacle : MonoBehaviour
{
    private float              _recycleY;
    private Action<GameObject> _returnCallback;
    private bool               _ready; // true solo después de Initialize()

    // ─── OnEnable: resetear estado al salir del pool ──────────────────────────

    private void OnEnable()
    {
        // El objeto se acaba de activar desde el pool; esperamos Initialize().
        _ready = false;
    }

    // ─── Inicialización por spawn ─────────────────────────────────────────────

    /// <summary>
    /// Llamar desde ObstacleSpawner después de activar el objeto.
    /// </summary>
    public void Initialize(float recycleY, Action<GameObject> returnCallback)
    {
        _recycleY       = recycleY;
        _returnCallback = returnCallback;
        _ready          = true;
    }

    // ─── Update: reciclaje por Y ──────────────────────────────────────────────

    private void Update()
    {
        if (!_ready) return;
        if (transform.position.y < _recycleY)
            Recycle();
    }

    // ─── Colisión con el jugador ──────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_ready) return;
        if (!other.CompareTag("Player")) return;

        other.GetComponent<RopePlayerController>()?.HitObstacle();
        Recycle();
    }

    // ─── Reciclaje ────────────────────────────────────────────────────────────

    private void Recycle()
    {
        _ready = false;              // evitar doble reciclaje en el mismo frame
        _returnCallback?.Invoke(gameObject);
    }
}
