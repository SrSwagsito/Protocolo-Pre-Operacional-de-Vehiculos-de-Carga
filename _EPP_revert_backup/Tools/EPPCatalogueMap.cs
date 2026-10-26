using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class EPPCatalogueMap
{
    private const string AssetPath = "Assets/Sprites/Icons/Catálogo de ropa y equipo de seguridad.png";
    private const int AlphaThreshold = 60;
    private const int Block = 20;

    public static void Execute()
    {
        string full = Path.Combine(Directory.GetParent(Application.dataPath).FullName, AssetPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(File.ReadAllBytes(full));

        int w = tex.width;
        int h = tex.height;
        Color32[] px = tex.GetPixels32();

        int cols = w / Block;
        int rows = h / Block;

        StringBuilder map = new StringBuilder();
        StringBuilder profile = new StringBuilder();

        for (int by = rows - 1; by >= 0; by--)
        {
            int y0 = by * Block;
            int y1 = Mathf.Min(h - 1, y0 + Block - 1);
            int rowFg = 0;

            for (int bx = 0; bx < cols; bx++)
            {
                int x0 = bx * Block;
                int x1 = Mathf.Min(w - 1, x0 + Block - 1);

                bool any = false;
                for (int y = y0; y <= y1 && !any; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        if (px[y * w + x].a <= AlphaThreshold) continue;
                        any = true;
                        break;
                    }
                }

                if (any)
                {
                    map.Append('#');
                    rowFg++;
                }
                else map.Append('.');
            }

            map.Append("  y").Append(y0).Append('-').Append(y1).Append(" cols=").Append(rowFg).Append('\n');
        }

        Debug.Log("EPPCatalogueMap: " + w + "x" + h + " blocks=" + cols + "x" + rows + "\n" + map + profile);
    }
}
