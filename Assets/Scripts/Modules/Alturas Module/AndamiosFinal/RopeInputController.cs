using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Maneja el input de la Fase 2 (rapel).
/// - Teclado: flechas Izquierda/Derecha o A/D (New Input System).
/// - UI: los botones llaman PressLeft(), PressRight(), ReleaseButton()
///   a través de RopeMovementButton.cs.
/// Expone OnDirectionChanged(-1, 0 o 1) para que RopePlayerController mueva al jugador.
/// </summary>
public class RopeInputController : MonoBehaviour
{
    /// <summary>Emite -1 (izquierda), 1 (derecha) o 0 (suelto).</summary>
    public event Action<float> OnDirectionChanged;

    private float _keyboardDirection;
    private float _buttonDirection;
    private float _lastSentDirection;
    private bool  _active;

    // ─── API pública para los botones UI ────────────────────────────────────

    public void PressLeft()
    {
        if (!_active) return;
        _buttonDirection = -1f;
        Evaluate();
    }

    public void PressRight()
    {
        if (!_active) return;
        _buttonDirection = 1f;
        Evaluate();
    }

    public void ReleaseButton()
    {
        _buttonDirection = 0f;
        Evaluate();
    }

    // ─── Activación ─────────────────────────────────────────────────────────

    public void SetActive(bool active)
    {
        _active = active;
        if (!active)
        {
            _buttonDirection  = 0f;
            _keyboardDirection = 0f;
            Send(0f);
        }
    }

    // ─── Teclado (New Input System) ──────────────────────────────────────────

    private void Update()
    {
        if (!_active || Keyboard.current == null) return;

        bool left  = Keyboard.current.leftArrowKey.isPressed  || Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed;

        float dir = 0f;
        if (left && !right)       dir = -1f;
        else if (right && !left)  dir =  1f;

        if (!Mathf.Approximately(dir, _keyboardDirection))
        {
            _keyboardDirection = dir;
            Evaluate();
        }
    }

    // ─── Lógica interna ──────────────────────────────────────────────────────

    /// <summary>El botón tiene prioridad sobre teclado si ambos están activos.</summary>
    private void Evaluate()
    {
        float final = !Mathf.Approximately(_buttonDirection, 0f)
            ? _buttonDirection
            : _keyboardDirection;

        Send(final);
    }

    private void Send(float dir)
    {
        if (Mathf.Approximately(dir, _lastSentDirection)) return;
        _lastSentDirection = dir;
        OnDirectionChanged?.Invoke(dir);
    }
}
