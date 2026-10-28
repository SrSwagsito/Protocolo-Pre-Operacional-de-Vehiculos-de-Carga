using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformInput : MonoBehaviour
{
    [SerializeField] PlatformMovement platform;

    private Vector2 keyboardDirection;

    private void Update()
    {
        if (platform == null) return;

        Vector2 direction = ReadKeyboard();
        if (direction == keyboardDirection) return;

        keyboardDirection = direction;
        platform.SetDirection(direction);
    }

    // Teclado (New Input System): flechas izquierda, derecha, arriba y abajo.
    private static Vector2 ReadKeyboard()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector2.zero;

        float horizontal = 0f;
        if (keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.rightArrowKey.isPressed) horizontal += 1f;

        float vertical = 0f;
        if (keyboard.downArrowKey.isPressed) vertical -= 1f;
        if (keyboard.upArrowKey.isPressed) vertical += 1f;

        return new Vector2(horizontal, vertical);
    }

    public void MoveUp()
    {
        platform.SetDirection(Vector2.up);
    }

    public void MoveDown()
    {
        platform.SetDirection(Vector2.down);
    }

   public void MoveLeft()
    {
        platform.SetDirection(Vector2.left);
    }

    public void MoveRight()
    {
        platform.SetDirection(Vector2.right);
    }

    public void Stop()
    {
        platform.SetDirection(Vector2.zero);
    }
}
