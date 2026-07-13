using UnityEngine;
using UnityEngine.EventSystems;

public class ClothSlot : DropZone
{
    [SerializeField]
    private DraggableCloth.ClothType acceptedType;

    [SerializeField]
    private CharacterDressController dressController;

    private DraggableCloth currentCloth;

    public override void OnDrop(PointerEventData eventData)
    {
        DraggableCloth cloth =
            eventData.pointerDrag?.GetComponent<DraggableCloth>();

        if (cloth == null)
            return;

        if (cloth.Cloth != acceptedType)
            return;

        if (currentCloth != null)
        {
            dressController.Unequip(currentCloth);
            currentCloth.ClearSlot();
            currentCloth.ReturnToStart();
        }

        currentCloth = cloth;

        cloth.SetInSlot(this);


        RectTransform rt = cloth.GetComponent<RectTransform>();

        cloth.transform.SetParent(transform, false);

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;


        dressController.Equip(cloth);
    }

    public void RemoveCloth()
    {
        if (currentCloth == null)
            return;

        dressController.Unequip(currentCloth);



        currentCloth = null;
    }

    public DraggableCloth GetCurrentCloth()
    {
        return currentCloth;
    }
}