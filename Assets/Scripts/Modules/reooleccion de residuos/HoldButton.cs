using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerMovement player;
    public bool isLeft;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLeft)
            player.MoveLeft();
        else
            player.MoveRight();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        player.StopMove();
    }
}