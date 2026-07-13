using System.Collections.Generic;
using UnityEngine;

public class CharacterDressController : MonoBehaviour
{
    private Dictionary<DraggableCloth.ClothType, DraggableCloth> equipped =
        new Dictionary<DraggableCloth.ClothType, DraggableCloth>();

    public void Equip(DraggableCloth cloth)
    {
        if (equipped.TryGetValue(cloth.Cloth, out DraggableCloth previous))
        {
            HideGraphics(previous);
        }

        equipped[cloth.Cloth] = cloth;

        ShowGraphics(cloth);
    }

    public void Unequip(DraggableCloth cloth)
    {
        if (!equipped.ContainsKey(cloth.Cloth))
            return;

        HideGraphics(cloth);

        equipped.Remove(cloth.Cloth);
    }

    private void ShowGraphics(DraggableCloth cloth)
    {
        foreach (CharacterGraphic graphic in cloth.CharacterGraphics)
        {
            if (graphic != null)
                graphic.Show();
        }
    }

    private void HideGraphics(DraggableCloth cloth)
    {
        foreach (CharacterGraphic graphic in cloth.CharacterGraphics)
        {
            if (graphic != null)
                graphic.Hide();
        }
    }
}