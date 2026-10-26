using System;
using System.Reflection;
using UnityEngine;

public static class EPPShowCategory
{
    public static void Execute(string category)
    {
        // Close any instruction popup so the catalogue is visible.
        foreach (MonoBehaviour behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (behaviour == null || behaviour.GetType().Name != "TutorialPanel") continue;

            FieldInfo field = behaviour.GetType().GetField("popupPanel",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            GameObject popup = field != null ? field.GetValue(behaviour) as GameObject : null;
            if (popup != null) popup.SetActive(false);
        }

        EPPVisualLayoutController controller = UnityEngine.Object.FindFirstObjectByType<EPPVisualLayoutController>();
        if (controller == null)
        {
            Debug.LogError("EPPShowCategory: no hay EPPVisualLayoutController.");
            return;
        }

        MethodInfo method = typeof(EPPVisualLayoutController)
            .GetMethod("SelectCategory", BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null)
        {
            Debug.LogError("EPPShowCategory: no se encontró SelectCategory.");
            return;
        }

        method.Invoke(controller, new object[] { category });
        Debug.Log("EPPShowCategory: " + category);
    }
}
