using UnityEngine;
using UnityEngine.EventSystems;

public class MoveButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [SerializeField] private PlatformMovement movement;
    [SerializeField] private Vector2 direction;

    public void OnPointerDown(PointerEventData eventData)
    {
        movement.SetDirection(direction);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        movement.SetDirection(Vector2.zero);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        movement.SetDirection(Vector2.zero);
    }
}