using UnityEngine;
using UnityEngine.EventSystems;

public class ClothSlot : DropZone
{
    [SerializeField]
    private DraggableCloth.ClothType acceptedType;

    [SerializeField]
    private CharacterDressController dressController;

    private DraggableCloth currentCloth;

    public DraggableCloth.ClothType AcceptedType => acceptedType;

    public bool IsFilled => currentCloth != null;

    private CharacterDressController Controller
    {
        get
        {
            if (dressController == null)
                dressController = FindFirstObjectByType<CharacterDressController>();
            return dressController;
        }
    }

    public override void OnDrop(PointerEventData eventData)
    {
        DraggableCloth cloth = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<DraggableCloth>()
            : null;

        if (cloth == null)
            return;

        if (TryEquip(cloth))
            cloth.ReturnToStart();
    }

    /// <summary>
    /// Equips the cloth and leaves its transform inside the catalogue card it was
    /// picked from, so the grid keeps the thumbnail and the selection checkmark.
    /// Returns false when the cloth does not belong to this slot.
    /// </summary>
    public bool TryEquip(DraggableCloth cloth)
    {
        if (cloth == null || cloth.Cloth != acceptedType)
            return false;

        if (currentCloth == cloth)
            return true;

        if (currentCloth != null)
        {
            DraggableCloth previous = currentCloth;
            currentCloth = null;
            previous.ClearSlot();
            CharacterDressController controller = Controller;
            if (controller != null) controller.Unequip(previous);
            previous.ReturnToStart();
        }

        currentCloth = cloth;
        cloth.SetInSlot(this);

        CharacterDressController dress = Controller;
        if (dress != null) dress.Equip(cloth);

        return true;
    }

    /// <summary>Called when a drag starts on the currently equipped item.</summary>
    public void RemoveCloth()
    {
        if (currentCloth == null)
            return;

        DraggableCloth previous = currentCloth;
        currentCloth = null;
        previous.ClearSlot();

        CharacterDressController dress = Controller;
        if (dress != null) dress.Unequip(previous);
    }

    /// <summary>Unequips and sends the item back to its catalogue card.</summary>
    public void UnequipCurrent()
    {
        if (currentCloth == null)
            return;

        DraggableCloth previous = currentCloth;
        RemoveCloth();
        previous.ReturnToStart();
    }

    public DraggableCloth GetCurrentCloth()
    {
        return currentCloth;
    }
}
