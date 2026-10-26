using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class EPPCatalogueProfile
{
    private const string AssetPath = "Assets/Sprites/Icons/Catálogo de ropa y equipo de seguridad.png";
    private const int AlphaThreshold = 60;
    private const int EmptyTolerance = 2;

    public static void Execute()
    {
        string full = Path.Combine(Directory.GetParent(Application.dataPath).FullName, AssetPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(File.ReadAllBytes(full));

        int w = tex.width;
        int h = tex.height;
        Color32[] px = tex.GetPixels32();

        StringBuilder log = new StringBuilder();

        log.Append("FILAS VACIAS (todo el ancho):\n");
        AppendEmptyRanges(log, px, w, h, 0, w - 1, true);

        log.Append("COLUMNAS VACIAS (todo el alto):\n");
        AppendEmptyRanges(log, px, w, h, 0, h - 1, false);

        Debug.Log("EPPCatalogueProfile: " + w + "x" + h + "\n" + log);
    }

    private static void AppendEmptyRanges(StringBuilder log, Color32[] px, int w, int h, int a0, int a1, bool rows)
    {
        int count = rows ? h : w;
        List<Vector2Int> ranges = new List<Vector2Int>();
        int start = -1;

        for (int i = 0; i < count; i++)
        {
            int fg = 0;
            int span = rows ? w : h;

            for (int j = 0; j < span; j++)
            {
                int x = rows ? j : i;
                int y = rows ? i : j;
                if (px[y * w + x].a <= AlphaThreshold) continue;
                fg++;
                if (fg > EmptyTolerance) break;
            }

            bool empty = fg <= EmptyTolerance;

            if (empty && start < 0) start = i;
            if (!empty && start >= 0)
            {
                ranges.Add(new Vector2Int(start, i - 1));
                start = -1;
            }
        }

        if (start >= 0) ranges.Add(new Vector2Int(start, count - 1));

        foreach (Vector2Int range in ranges)
            log.Append("  ").Append(range.x).Append("..").Append(range.y)
                .Append(" (len ").Append(range.y - range.x + 1).Append(")\n");
    }
}
