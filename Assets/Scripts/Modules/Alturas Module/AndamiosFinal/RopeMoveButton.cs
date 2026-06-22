using UnityEngine;
using UnityEngine.EventSystems;

public class RopeMoveButton :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField]
    private RopePlayerController player;

    [SerializeField]
    private float direction;

    private bool pressed;

    private void Update()
    {
        if (pressed)
        {
            player.SetDirection(direction);
        }
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        pressed = true;
    }

    public void OnPointerUp(
        PointerEventData eventData)
    {
        pressed = false;
    }
}