using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class EPPFixItemSizes
{
    public static void Execute()
    {
        GameObject ui = GameObject.Find("UI");
        Transform game = ui != null ? ui.transform.Find("Game UI") : null;
        if (game == null)
        {
            Debug.LogError("EPPFixItemSizes: no se encontró UI/Game UI.");
            return;
        }

        DraggableCloth[] cloths = game.GetComponentsInChildren<DraggableCloth>(true);
        StringBuilder report = new StringBuilder();
        report.Append("EPPFixItemSizes: ").Append(cloths.Length).Append(" items\n");

        foreach (DraggableCloth cloth in cloths)
        {
            RectTransform rect = cloth.transform as RectTransform;
            Image image = cloth.GetComponent<Image>();
            if (rect == null || image == null || image.sprite == null) continue;

            Rect r = image.sprite.rect;
            Vector2 size = new Vector2(r.width, r.height);

            report.Append("- ").Append(cloth.name).Append(' ').Append(rect.sizeDelta.ToString("0"))
                .Append(" -> ").Append(size.ToString("0")).Append('\n');

            Undo.RecordObject(rect, "EPP item size");
            rect.sizeDelta = size;
        }

        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
        EditorSceneManager.SaveScene(game.gameObject.scene);
        Debug.Log(report.ToString());
    }
}
