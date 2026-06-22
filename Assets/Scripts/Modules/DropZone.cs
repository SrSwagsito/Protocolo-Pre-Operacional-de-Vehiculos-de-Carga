using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{

    public virtual void OnDrop(PointerEventData eventData)
    {
        DraggableObject draggable =
            eventData.pointerDrag.GetComponent<DraggableObject>();

        if (draggable == null)
            return;

        //if (draggable.typeObject == ValidateObject.correct)
        //{
        //    Debug.Log("Objeto correcto");

            draggable.transform.SetParent(transform);
            draggable.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        //}
        //else
        //{
        //    Debug.Log("Objeto incorrecto");

        //    draggable.ReturnToOriginalPosition();
        //}
    }
}