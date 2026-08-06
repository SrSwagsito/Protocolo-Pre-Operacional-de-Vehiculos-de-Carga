using UnityEngine;
using System;

/// <summary>
/// Controla al jugador durante la Fase 2 (rapel).
///
/// Cambios respecto a la versión anterior:
/// - El jugador se mueve manualmente en X con RopeInputController
///   (flechas/AD + botones UI).
/// - La lógica de "vidas" está delegada a LifeSystem; este script
///   solo llama LoseLife() y escucha OnLivesEmpty para disparar OnFail.
/// - SetDirection() auto-move eliminado; el movimiento es siempre manual.
/// </summary>
public class RopePlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 3f;

    // ─── Eventos ──────────────────────────────────────────────────────────────
    public event Action OnSwingEnded;
    public event Action OnFail;

    // ─── Estado interno ───────────────────────────────────────────────────────
    private float              _direction;
    private bool               _canMove;
    private Vector3            _checkpoint;
    private LifeSystemFinal         _lifeSystem;
    private RopeInputController _inputController;

    [Header("Límites horizontales")]
    public float leftLimit = -5f;
    public float rightLimit = 5f;

    // ─── Inicialización ───────────────────────────────────────────────────────

    /// <summary>Llamar desde Module4Manager antes de EnableMovement().</summary>
    public void Initialize(LifeSystemFinal lifeSystem, RopeInputController inputController)
    {
        _lifeSystem       = lifeSystem;
        _inputController  = inputController;

        _lifeSystem.OnLivesEmpty         += HandleLivesEmpty;
        _inputController.OnDirectionChanged += OnDirectionInput;
    }

    // ─── Control de movimiento ────────────────────────────────────────────────

    public void EnableMovement()
    {
        _checkpoint = transform.position;
        _canMove    = true;
        _inputController?.SetActive(true);
    }

    public void DisableMovement()
    {
        _canMove  = false;
        _direction = 0f;
        _inputController?.SetActive(false);
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!_canMove) return;
        transform.Translate(Vector3.right * (_direction * moveSpeed * Time.deltaTime));

        // Clamp posición horizontal
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);
        transform.position = pos;
    }

    // ─── Eventos de gameplay ──────────────────────────────────────────────────

    /// <summary>Llamado por RopeGoal al entrar en el trigger final.</summary>
    public void ReachGoal()
    {
        DisableMovement();
        OnSwingEnded?.Invoke();
    }

    /// <summary>Llamado por FallingObstacle al colisionar con el jugador.</summary>
    public void HitObstacle()
    {
        _lifeSystem?.LoseLife();

        // Si aún quedan vidas, volver al checkpoint
        if (_lifeSystem != null && _lifeSystem.CurrentLives > 0)
            Respawn();
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private void Respawn() => transform.position = _checkpoint;

    private void OnDirectionInput(float dir) => _direction = dir;

    private void HandleLivesEmpty()
    {
        DisableMovement();
        OnFail?.Invoke();
    }

    private void OnDestroy()
    {
        if (_lifeSystem != null)
            _lifeSystem.OnLivesEmpty -= HandleLivesEmpty;
        if (_inputController != null)
            _inputController.OnDirectionChanged -= OnDirectionInput;
    }
}
