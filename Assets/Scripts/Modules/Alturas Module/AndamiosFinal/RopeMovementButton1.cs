using UnityEngine;
using UnityEngine.EventSystems;

public class RopeMovementButtonModule : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Direction { Left, Right }
    public Direction direction;
    public RopeInputController inputController;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (direction == Direction.Left) inputController.PressLeft();
        else inputController.PressRight();
    }

    public void OnPointerUp(PointerEventData eventData) => inputController.ReleaseButton();
    public void OnPointerExit(PointerEventData eventData) => inputController.ReleaseButton();
}