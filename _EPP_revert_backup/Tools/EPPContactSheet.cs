using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class EPPContactSheet
{
    private const string AtlasPath = "Assets/Sprites/Icons/Catálogo de ropa y equipo de seguridad.png";
    private const string OutDir = "Assets/EPPDebug";
    private const string OutPath = "Assets/EPPDebug/contacto.png";

    private const int Cell = 200;
    private const int Margin = 10;
    private const int Columns = 6;

    public static void Execute()
    {
        GameObject ui = GameObject.Find("UI");
        Transform game = ui != null ? ui.transform.Find("Game UI") : null;
        if (game == null)
        {
            Debug.LogError("EPPContactSheet: no se encontró UI/Game UI.");
            return;
        }

        DraggableCloth[] cloths = game.GetComponentsInChildren<DraggableCloth>(true);

        Texture2D atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!atlas.LoadImage(File.ReadAllBytes(Path.GetFullPath(AtlasPath))))
        {
            Debug.LogError("EPPContactSheet: no se pudo leer el atlas.");
            return;
        }

        int aw = atlas.width;
        Color32[] apx = atlas.GetPixels32();

        int rows = Mathf.CeilToInt(cloths.Length / (float)Columns);
        int width = Columns * Cell + Margin * 2;
        int height = rows * Cell + Margin * 2;

        Color32[] outPx = new Color32[width * height];
        for (int i = 0; i < outPx.Length; i++) outPx[i] = new Color32(235, 235, 235, 255);

        StringBuilder report = new StringBuilder();
        report.Append("EPPContactSheet: ").Append(cloths.Length).Append(" items, ").Append(Columns)
            .Append(" columnas, celda ").Append(Cell).Append("px\n");

        for (int index = 0; index < cloths.Length; index++)
        {
            Image image = cloths[index].GetComponent<Image>();
            Sprite sprite = image != null ? image.sprite : null;

            int col = index % Columns;
            int row = index / Columns;
            int cellX = Margin + col * Cell;
            int cellY = height - Margin - (row + 1) * Cell;

            report.Append(index.ToString("00")).Append(" c").Append(col).Append(" r").Append(row)
                .Append(" ").Append(cloths[index].name);

            if (sprite == null)
            {
                report.Append(" SIN SPRITE\n");
                continue;
            }

            Rect r = sprite.rect;
            report.Append(" rect(").Append(r.x).Append(',').Append(r.y).Append(' ')
                .Append(r.width).Append('x').Append(r.height).Append(')')
                .Append(" delta=").Append(cloths[index].transform is RectTransform rt ? rt.sizeDelta.ToString("0") : "?")
                .Append('\n');

            float scale = Mathf.Min((Cell - 8f) / r.width, (Cell - 8f) / r.height);
            float offX = (Cell - r.width * scale) * 0.5f;
            float offY = (Cell - r.height * scale) * 0.5f;

            for (int y = 0; y < Cell; y++)
            {
                for (int x = 0; x < Cell; x++)
                {
                    int sx = Mathf.FloorToInt(r.x + (x - offX) / scale);
                    int sy = Mathf.FloorToInt(r.y + (y - offY) / scale);

                    if (sx < 0 || sx >= aw || sy < 0 || sy >= atlas.height) continue;
                    if (x < offX || x >= offX + r.width * scale) continue;
                    if (y < offY || y >= offY + r.height * scale) continue;

                    Color32 c = apx[sy * aw + sx];
                    if (c.a < 24) continue;

                    int px = cellX + x;
                    int py = cellY + y;
                    if (px < 0 || px >= width || py < 0 || py >= height) continue;

                    outPx[py * width + px] = new Color32(c.r, c.g, c.b, 255);
                }
            }
        }

        Texture2D outTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        outTex.SetPixels32(outPx);
        outTex.Apply();

        if (!Directory.Exists(Path.GetFullPath(OutDir))) Directory.CreateDirectory(Path.GetFullPath(OutDir));
        File.WriteAllBytes(Path.GetFullPath(OutPath), outTex.EncodeToPNG());
        AssetDatabase.ImportAsset(OutPath, ImportAssetOptions.ForceUpdate);

        Debug.Log(report.ToString());
    }
}
