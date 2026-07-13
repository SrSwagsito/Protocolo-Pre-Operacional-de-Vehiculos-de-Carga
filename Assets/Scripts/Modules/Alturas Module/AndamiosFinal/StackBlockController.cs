using System;
using UnityEngine;
using UnityEngine;
using UnityEngine.InputSystem;

public class StackInputController : MonoBehaviour
{
    public event Action OnDropInput;

    private bool _enabled;

    private void Update()
    {
        if (!_enabled)
            return;

        bool mousePressed =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        bool touchPressed =
            Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (mousePressed || touchPressed)
        {
            OnDropInput?.Invoke();
        }
    }

    public void SetActive(bool active) => _enabled = active;
}