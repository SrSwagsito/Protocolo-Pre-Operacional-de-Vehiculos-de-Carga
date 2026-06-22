using UnityEngine;

public class PlatformInput : MonoBehaviour
{
    [SerializeField] PlatformMovement platform;

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
