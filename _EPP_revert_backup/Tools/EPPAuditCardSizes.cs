using System.Text;
using UnityEngine;

public static class EPPAuditCardSizes
{
    public static void Execute()
    {
        DraggableCloth[] cloths = Object.FindObjectsByType<DraggableCloth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        StringBuilder sb = new StringBuilder();
        sb.Append("EPPAuditCardSizes: ").Append(cloths.Length).Append(" items\n");

        int tooSmall = 0;

        foreach (DraggableCloth cloth in cloths)
        {
            RectTransform rect = cloth.transform as RectTransform;
            if (rect == null) continue;

            float biggest = Mathf.Max(rect.sizeDelta.x, rect.sizeDelta.y);
            if (biggest < 100f) tooSmall++;

            sb.Append("- ").Append(cloth.name)
                .Append(" size=").Append(rect.sizeDelta.ToString("0.0"))
                .Append(" sizeRatio=").Append((rect.sizeDelta.x / Mathf.Max(1f, rect.sizeDelta.y)).ToString("0.00"))
                .Append(" parent=").Append(rect.parent != null ? rect.parent.name : "NULL")
                .Append(biggest < 100f ? "  <-- PEQUENO" : string.Empty)
                .Append('\n');
        }

        sb.Append("demasiado-pequenos=").Append(tooSmall).Append('\n');
        Debug.Log(sb.ToString());
    }
}
