using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EPPUpdateTutorial
{
    private const string NewText =
        "• Para aprobar: los 7 EPP correctos y ninguna zona vacía.\n\n" +
        "• Cada categoría ofrece 4 opciones y solo una es correcta: casco blanco, camisa EPP verde, " +
        "pantalón EPP verde, guantes azules, botas negras, gafas transparentes y arnés amarillo. " +
        "El resto son distractores: si los equipas, el módulo queda desaprobado.\n\n" +
        "• Puntaje: 100 con los 7 aciertos; cada elemento incorrecto descuenta.\n\n" +
        "• Cierra esta ventana con la X para comenzar.";

    public static void Execute()
    {
        int changed = 0;

        foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text == null || string.IsNullOrEmpty(text.text)) continue;
            if (!text.text.Contains("son distractores")) continue;

            Undo.RecordObject(text, "EPP tutorial text");
            text.text = NewText;
            changed++;
        }

        if (changed > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        Debug.Log("EPPUpdateTutorial: textos actualizados=" + changed);
    }
}
