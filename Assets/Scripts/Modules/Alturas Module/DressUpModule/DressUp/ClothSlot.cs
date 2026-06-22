using UnityEngine;
using UnityEngine.EventSystems;

public class ClothSlot : DropZone
{
    private DraggableCloth currentCloth;

    public override void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"OnDrop ejecutado en slot {name}");

        DraggableCloth droppedCloth = eventData.pointerDrag?.GetComponent<DraggableCloth>();

        if (droppedCloth != null)
        {
            currentCloth = droppedCloth;
            droppedCloth.transform.SetParent(transform);
            droppedCloth.transform.localPosition = Vector3.zero;
            Debug.Log($"Prenda {droppedCloth.name} colocada en slot {name}");
        }
        else
        {
            Debug.LogWarning($"El objeto arrastrado no tiene DraggableCloth. Objeto: {eventData.pointerDrag?.name}");
        }
    }

    public DraggableCloth GetCurrentCloth()
    {
        return currentCloth;
    }
}
