using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class EPPSliceCatalogue
{
    private const string AssetPath = "Assets/Sprites/Icons/Catálogo de ropa y equipo de seguridad.png";
    private const int AlphaThreshold = 60;

    // y is bottom-origin. Bands and item counts come from the empty-row profile.
    private static readonly int[][] Bands =
    {
        new[] { 21, 144, 12 },
        new[] { 155, 214, 8 },
        new[] { 222, 339, 8 },
        new[] { 340, 518, 8 },
        new[] { 522, 720, 8 },
        new[] { 730, 908, 8 }
    };

    private static readonly string[][] BandNames =
    {
        new[] { "Bota negra", "Bota marron", "Bota azul", "Bota amarilla", "Chancla verde", "Chancla rosa", "Chancla azul", "Chancla morada", "Guante azul", "Guante negro", "Guante gris", "Guante verde" },
        new[] { "Reloj marron", "Reloj negro", "Reloj verde", "Reloj gris", "Aretes dorados", "Aretes azules", "Aretes estrellas", "Aretes verdes" },
        new[] { "Gafas transparentes", "Gafas amarillas", "Gafas azules", "Gafas oscuras", "Casco blanco", "Casco amarillo", "Casco azul", "Casco verde" },
        new[] { "Arnes amarillo", "Arnes rojo", "Arnes azul", "Arnes verde", "Chaleco verde", "Chaleco naranja", "Chaleco azul", "Chaleco azul marino" },
        new[] { "Pantalon azul", "Pantalon caqui", "Pantalon gris", "Pantalon oliva", "Pantalon EPP verde", "Pantalon EPP azul", "Pantalon EPP naranja", "Pantalon EPP azul marino" },
        new[] { "Camisa blanca", "Camisa negra", "Camisa azul", "Camisa amarilla", "Camisa EPP verde", "Camisa EPP azul", "Camisa EPP naranja", "Camisa EPP azul marino" }
    };

    private class Entry
    {
        public string Name;
        public int X;
        public int Y;
        public int W;
        public int H;
        public long InternalId;
    }

    public static void Execute()
    {
        string pngPath = Path.GetFullPath(AssetPath);
        string metaPath = pngPath + ".meta";

        if (!File.Exists(pngPath))
        {
            Debug.LogError("EPPSliceCatalogue: no existe " + pngPath);
            return;
        }

        if (!File.Exists(metaPath))
        {
            Debug.LogError("EPPSliceCatalogue: no existe el .meta " + metaPath);
            return;
        }

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(File.ReadAllBytes(pngPath)))
        {
            Debug.LogError("EPPSliceCatalogue: LoadImage falló.");
            return;
        }

        int w = tex.width;
        int h = tex.height;
        Color32[] px = tex.GetPixels32();
        if (px == null || px.Length < w * h)
        {
            Debug.LogError("EPPSliceCatalogue: no se pudieron leer los píxeles (" + w + "x" + h + ").");
            return;
        }

        List<Entry> entries = new List<Entry>();
        StringBuilder report = new StringBuilder();
        report.Append("textura ").Append(w).Append('x').Append(h).Append('\n');

        for (int b = 0; b < Bands.Length; b++)
        {
            int y0 = Bands[b][0];
            int y1 = Mathf.Min(Bands[b][1], h - 1);
            int count = Bands[b][2];
            string[] names = BandNames[b];

            int minX = w - 1;
            int maxX = 0;

            for (int y = y0; y <= y1; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    if (px[row + x].a <= AlphaThreshold) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                }
            }

            if (maxX <= minX)
            {
                report.Append("banda ").Append(b).Append(": VACIA\n");
                continue;
            }

            float pitch = (maxX - minX + 1f) / count;
            report.Append("banda ").Append(b).Append(" x").Append(minX).Append("..").Append(maxX)
                .Append(" pitch ").Append(pitch.ToString("0.0")).Append(": ");

            // Items sit side by side, so every item is an uninterrupted run of
            // occupied columns. Splitting on empty columns keeps the neighbour's
            // pixels out of the sprite, which equal-width cells cannot guarantee.
            int[][] runs = ColumnRuns(px, w, y0, y1, count);

            for (int c = 0; c < count; c++)
            {
                int cellX0;
                int cellX1;

                if (runs != null && c < runs.Length)
                {
                    cellX0 = runs[c][0];
                    cellX1 = runs[c][1];
                }
                else
                {
                    cellX0 = Mathf.RoundToInt(minX + c * pitch);
                    cellX1 = Mathf.Min(Mathf.RoundToInt(minX + (c + 1) * pitch) - 1, w - 1);
                }

                int foundX0 = cellX1;
                int foundX1 = cellX0;
                int foundY0 = y1;
                int foundY1 = y0;

                for (int y = y0; y <= y1; y++)
                {
                    int row = y * w;
                    for (int x = cellX0; x <= cellX1; x++)
                    {
                        if (px[row + x].a <= AlphaThreshold) continue;
                        if (x < foundX0) foundX0 = x;
                        if (x > foundX1) foundX1 = x;
                        if (y < foundY0) foundY0 = y;
                        if (y > foundY1) foundY1 = y;
                    }
                }

                if (foundX1 < foundX0 || foundY1 < foundY0)
                {
                    foundX0 = cellX0;
                    foundX1 = cellX1;
                    foundY0 = y0;
                    foundY1 = y1;
                }

                Entry entry = new Entry
                {
                    Name = c < names.Length ? names[c] : "Item " + b + "_" + c,
                    X = foundX0,
                    Y = foundY0,
                    W = foundX1 - foundX0 + 1,
                    H = foundY1 - foundY0 + 1,
                    InternalId = -(1000000L + entries.Count * 7919L)
                };

                entries.Add(entry);
                report.Append(entry.Name).Append("(").Append(entry.X).Append(',').Append(entry.Y)
                    .Append(' ').Append(entry.W).Append('x').Append(entry.H).Append(") ");
            }

            report.Append('\n');
        }

        if (entries.Count == 0)
        {
            Debug.LogError("EPPSliceCatalogue: no se calculó ninguna entrada.");
            return;
        }

        string original = File.ReadAllText(metaPath);
        string nl = original.Contains("\r\n") ? "\r\n" : "\n";

        string backupPath = metaPath + ".bak";
        if (!File.Exists(backupPath)) File.WriteAllText(backupPath, original, new UTF8Encoding(false));

        string newText = ReplaceInternalIdTable(original, entries, nl);
        newText = ReplaceSpriteList(newText, entries, nl);
        newText = ReplaceNameFileIdTable(newText, entries, nl);

        if (newText == null)
        {
            Debug.LogError("EPPSliceCatalogue: no se pudo localizar la estructura del .meta.");
            return;
        }

        File.WriteAllText(metaPath, newText, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(
            AssetPath,
            ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

        int spriteCount = 0;
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(AssetPath))
            if (asset is Sprite) spriteCount++;

        string text = "entries=" + entries.Count + " sprites=" + spriteCount + "\n" + report;
        Debug.Log("EPPSliceCatalogue: " + text);

        if (!Directory.Exists(Path.GetFullPath("Assets/EPPDebug")))
            Directory.CreateDirectory(Path.GetFullPath("Assets/EPPDebug"));

        File.WriteAllText(Path.GetFullPath("Assets/EPPDebug/slice-report.txt"), text);
    }

    private static int[][] ColumnRuns(Color32[] px, int w, int y0, int y1, int count)
    {
        bool[] occupied = new bool[w];

        for (int y = y0; y <= y1; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
                if (px[row + x].a > AlphaThreshold) occupied[x] = true;
        }

        List<int[]> runs = new List<int[]>();
        int i = 0;
        while (i < w)
        {
            if (!occupied[i])
            {
                i++;
                continue;
            }

            int start = i;
            while (i < w && occupied[i]) i++;
            runs.Add(new[] { start, i - 1 });
        }

        if (runs.Count < count) return null;

        // Merge the closest neighbours until only the expected items remain, so a
        // garment that has an interior gap is rejoined instead of split in two.
        while (runs.Count > count)
        {
            int best = -1;
            int bestGap = int.MaxValue;

            for (int r = 0; r < runs.Count - 1; r++)
            {
                int gap = runs[r + 1][0] - runs[r][1];
                if (gap >= bestGap) continue;
                bestGap = gap;
                best = r;
            }

            if (best < 0) return null;

            runs[best] = new[] { runs[best][0], runs[best + 1][1] };
            runs.RemoveAt(best + 1);
        }

        return runs.ToArray();
    }

    private static string ReplaceInternalIdTable(string text, List<Entry> entries, string nl)
    {
        const string startToken = "  internalIDToNameTable:";
        int start = text.IndexOf(startToken, StringComparison.Ordinal);
        if (start < 0) return null;

        int cursor = start + startToken.Length;
        if (text.Substring(cursor, nl.Length) == nl) cursor += nl.Length;

        // Consume the existing "- first: / 213: id / second: name" triplets.
        while (true)
        {
            if (!text.Substring(cursor).StartsWith("  - first:", StringComparison.Ordinal)) break;

            // advance past the three lines of this triplet
            int lineEnd = cursor;
            for (int i = 0; i < 3; i++)
            {
                lineEnd = text.IndexOf(nl, lineEnd, StringComparison.Ordinal);
                if (lineEnd < 0) return null;
                lineEnd += nl.Length;
            }

            cursor = lineEnd;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append(text, 0, start);
        sb.Append(startToken).Append(nl);
        for (int i = 0; i < entries.Count; i++)
        {
            sb.Append("  - first:").Append(nl);
            sb.Append("      213: ").Append(entries[i].InternalId).Append(nl);
            sb.Append("    second: \"").Append(entries[i].Name).Append('"').Append(nl);
        }

        sb.Append(text, cursor, text.Length - cursor);
        return sb.ToString();
    }

    private static string ReplaceSpriteList(string text, List<Entry> entries, string nl)
    {
        const string startToken = "    sprites:";
        const string endToken = "    outline: []";

        int start = text.IndexOf(startToken, StringComparison.Ordinal);
        if (start < 0) return null;

        int listStart = start + startToken.Length;
        if (text.Substring(listStart, nl.Length) == nl) listStart += nl.Length;

        int end = text.IndexOf(nl + endToken, listStart, StringComparison.Ordinal);
        if (end < 0) return null;
        end += nl.Length;

        StringBuilder sb = new StringBuilder();
        sb.Append(text, 0, listStart);

        for (int i = 0; i < entries.Count; i++)
        {
            Entry e = entries[i];
            sb.Append("    - serializedVersion: 2").Append(nl);
            sb.Append("      name: \"").Append(e.Name).Append('"').Append(nl);
            sb.Append("      rect:").Append(nl);
            sb.Append("        serializedVersion: 2").Append(nl);
            sb.Append("        x: ").Append(e.X).Append(nl);
            sb.Append("        y: ").Append(e.Y).Append(nl);
            sb.Append("        width: ").Append(e.W).Append(nl);
            sb.Append("        height: ").Append(e.H).Append(nl);
            sb.Append("      alignment: 0").Append(nl);
            sb.Append("      pivot: {x: 0.5, y: 0.5}").Append(nl);
            sb.Append("      border: {x: 0, y: 0, z: 0, w: 0}").Append(nl);
            sb.Append("      customData: ").Append(nl);
            sb.Append("      outline: []").Append(nl);
            sb.Append("      physicsShape: []").Append(nl);
            sb.Append("      tessellationDetail: -1").Append(nl);
            sb.Append("      bones: []").Append(nl);
            sb.Append("      spriteID: ").Append(Guid.NewGuid().ToString("N")).Append(nl);
            sb.Append("      internalID: ").Append(e.InternalId).Append(nl);
            sb.Append("      vertices: []").Append(nl);
            sb.Append("      indices: ").Append(nl);
            sb.Append("      edges: []").Append(nl);
            sb.Append("      weights: []").Append(nl);
        }

        sb.Append(text, end, text.Length - end);
        return sb.ToString();
    }

    private static string ReplaceNameFileIdTable(string text, List<Entry> entries, string nl)
    {
        const string startToken = "    nameFileIdTable:";
        const string endToken = "  mipmapLimitGroupName:";

        int start = text.IndexOf(startToken, StringComparison.Ordinal);
        if (start < 0) return null;

        int listStart = start + startToken.Length;
        if (text.Substring(listStart, nl.Length) == nl) listStart += nl.Length;

        int end = text.IndexOf(nl + endToken, listStart, StringComparison.Ordinal);
        if (end < 0) return null;
        end += nl.Length;

        StringBuilder sb = new StringBuilder();
        sb.Append(text, 0, listStart);
        for (int i = 0; i < entries.Count; i++)
            sb.Append("      \"").Append(entries[i].Name).Append("\": ").Append(entries[i].InternalId).Append(nl);

        sb.Append(text, end, text.Length - end);
        return sb.ToString();
    }
}
