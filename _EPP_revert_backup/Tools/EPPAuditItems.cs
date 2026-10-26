using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class EPPAuditItems
{
    public static void Execute()
    {
        GameObject ui = GameObject.Find("UI");
        Transform game = ui != null ? ui.transform.Find("Game UI") : null;
        if (game == null)
        {
            Debug.LogError("EPPAuditItems: no se encontró UI/Game UI.");
            return;
        }

        DraggableCloth[] cloths = game.GetComponentsInChildren<DraggableCloth>(true);
        StringBuilder sb = new StringBuilder();
        sb.Append("EPPAuditItems: total=").Append(cloths.Length).Append('\n');

        int missingAtlas = 0;

        foreach (DraggableCloth cloth in cloths)
        {
            Image image = cloth.GetComponent<Image>();
            Sprite sprite = image != null ? image.sprite : null;

            string atlas = "sin-sprite";
            if (sprite != null)
            {
                string path = AssetDatabase.GetAssetPath(sprite);
                atlas = path == "Assets/Sprites/Icons/Catálogo de ropa y equipo de seguridad.png"
                    ? "NUEVO"
                    : path;
            }

            if (atlas != "NUEVO") missingAtlas++;

            sb.Append("- ").Append(cloth.name)
                .Append(" | ").Append(cloth.Cloth)
                .Append(cloth.IsCorrect ? " CORRECTA" : " distractor")
                .Append(" | sprite=").Append(sprite != null ? sprite.name : "NULL")
                .Append(" | atlas=").Append(atlas)
                .Append(" | layers=").Append(cloth.CharacterGraphics != null ? cloth.CharacterGraphics.Length : 0)
                .Append('\n');
        }

        sb.Append("fuera-del-atlas=").Append(missingAtlas).Append('\n');
        Debug.Log(sb.ToString());
    }
}
