using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class EPPRebuildItems
{
    private const string AtlasPath = "Assets/Sprites/Icons/Catálogo de ropa y equipo de seguridad.png";

    private class Spec
    {
        public string OldName;
        public string NewName;
        public string SpriteName;
        public DraggableCloth.ClothType Type;
        public bool Correct;
        public string[] Layers;
    }

    private static readonly Spec[] Specs =
    {
        new Spec { OldName = "Casco", NewName = "Casco blanco", SpriteName = "Casco blanco", Type = DraggableCloth.ClothType.Helmet, Correct = true, Layers = new[] { "HelmetLayer" } },
        new Spec { OldName = "Balaca sprite", NewName = "Casco amarillo", SpriteName = "Casco amarillo", Type = DraggableCloth.ClothType.Helmet, Layers = new[] { "BalacaLayer (1)" } },
        new Spec { NewName = "Casco azul", SpriteName = "Casco azul", Type = DraggableCloth.ClothType.Helmet, Layers = new[] { "BalacaLayer (1)" } },
        new Spec { NewName = "Casco verde", SpriteName = "Casco verde", Type = DraggableCloth.ClothType.Helmet, Layers = new[] { "BalacaLayer (1)" } },

        new Spec { OldName = "Camisa del unifrome", NewName = "Camisa EPP verde", SpriteName = "Camisa EPP verde", Type = DraggableCloth.ClothType.Shirt, Correct = true, Layers = new[] { "ShirtLayer" } },
        new Spec { OldName = "Camisa", NewName = "Camisa EPP azul", SpriteName = "Camisa EPP azul", Type = DraggableCloth.ClothType.Shirt, Layers = new[] { "CasualShirtLayer" } },
        new Spec { NewName = "Camisa EPP naranja", SpriteName = "Camisa EPP naranja", Type = DraggableCloth.ClothType.Shirt, Layers = new[] { "CasualShirtLayer" } },
        new Spec { NewName = "Camisa EPP azul marino", SpriteName = "Camisa EPP azul marino", Type = DraggableCloth.ClothType.Shirt, Layers = new[] { "CasualShirtLayer" } },

        new Spec { OldName = "Pantalon del uniforme", NewName = "Pantalon EPP verde", SpriteName = "Pantalon EPP verde", Type = DraggableCloth.ClothType.Pants, Correct = true, Layers = new[] { "PantsLayer" } },
        new Spec { OldName = "Jean sprite", NewName = "Pantalon EPP azul", SpriteName = "Pantalon EPP azul", Type = DraggableCloth.ClothType.Pants, Layers = new[] { "JeansLayer" } },
        new Spec { NewName = "Pantalon EPP naranja", SpriteName = "Pantalon EPP naranja", Type = DraggableCloth.ClothType.Pants, Layers = new[] { "JeansLayer" } },
        new Spec { NewName = "Pantalon EPP azul marino", SpriteName = "Pantalon EPP azul marino", Type = DraggableCloth.ClothType.Pants, Layers = new[] { "JeansLayer" } },

        new Spec { OldName = "Guantes", NewName = "Guante azul", SpriteName = "Guante azul", Type = DraggableCloth.ClothType.Gloves, Correct = true, Layers = new[] { "GlovesLayer" } },
        new Spec { OldName = "Reloj", NewName = "Guante negro", SpriteName = "Guante negro", Type = DraggableCloth.ClothType.Gloves, Layers = new[] { "WatchLayer" } },
        new Spec { NewName = "Guante gris", SpriteName = "Guante gris", Type = DraggableCloth.ClothType.Gloves, Layers = new[] { "WatchLayer" } },
        new Spec { NewName = "Guante verde", SpriteName = "Guante verde", Type = DraggableCloth.ClothType.Gloves, Layers = new[] { "WatchLayer" } },

        new Spec { OldName = "Botas", NewName = "Bota negra", SpriteName = "Bota negra", Type = DraggableCloth.ClothType.Boots, Correct = true, Layers = new[] { "BootsLayer" } },
        new Spec { OldName = "Chanclas", NewName = "Bota marron", SpriteName = "Bota marron", Type = DraggableCloth.ClothType.Boots, Layers = new[] { "Chanclas Layer" } },
        new Spec { NewName = "Bota azul", SpriteName = "Bota azul", Type = DraggableCloth.ClothType.Boots, Layers = new[] { "Chanclas Layer" } },
        new Spec { NewName = "Bota amarilla", SpriteName = "Bota amarilla", Type = DraggableCloth.ClothType.Boots, Layers = new[] { "Chanclas Layer" } },

        new Spec { OldName = "Lentes", NewName = "Gafas transparentes", SpriteName = "Gafas transparentes", Type = DraggableCloth.ClothType.Glasses, Correct = true, Layers = new[] { "GlassesLayer" } },
        new Spec { OldName = "Aretes", NewName = "Gafas oscuras", SpriteName = "Gafas oscuras", Type = DraggableCloth.ClothType.Glasses, Layers = new[] { "EarringsLayer (1)" } },
        new Spec { OldName = "Arnes", NewName = "Arnes amarillo", SpriteName = "Arnes amarillo", Type = DraggableCloth.ClothType.Harness, Correct = true, Layers = new[] { "HarnessLayer" } },
        new Spec { NewName = "Arnes rojo", SpriteName = "Arnes rojo", Type = DraggableCloth.ClothType.Harness, Layers = new string[0] }
    };

    public static void Execute()
    {
        GameObject ui = GameObject.Find("UI");
        Transform game = ui != null ? ui.transform.Find("Game UI") : null;
        if (game == null)
        {
            Debug.LogError("EPPRebuildItems: no se encontró UI/Game UI.");
            return;
        }

        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(AtlasPath))
        {
            Sprite sprite = asset as Sprite;
            if (sprite != null) sprites[sprite.name] = sprite;
        }

        StringBuilder report = new StringBuilder();
        report.Append("sprites en atlas: ").Append(sprites.Count).Append('\n');

        int created = 0;
        int reskinned = 0;

        foreach (Spec spec in Specs)
        {
            if (!sprites.TryGetValue(spec.SpriteName, out Sprite sprite))
            {
                report.Append("FALTA SPRITE: ").Append(spec.SpriteName).Append('\n');
                continue;
            }

            DraggableCloth cloth = null;

            if (!string.IsNullOrEmpty(spec.OldName))
            {
                Transform existing = game.Find(spec.OldName);
                if (existing != null) cloth = existing.GetComponent<DraggableCloth>();
            }

            if (cloth != null)
            {
                Undo.RecordObject(cloth.gameObject, "EPP re-skin");
                cloth.gameObject.name = spec.NewName;

                Image image = cloth.GetComponent<Image>();
                if (image != null)
                {
                    Undo.RecordObject(image, "EPP re-skin sprite");
                    image.sprite = sprite;
                    image.preserveAspect = true;
                    image.type = Image.Type.Simple;
                    image.color = Color.white;
                }

                reskinned++;
            }
            else
            {
                GameObject go = new GameObject(spec.NewName, typeof(RectTransform), typeof(CanvasRenderer));
                go.layer = game.gameObject.layer;
                go.transform.SetParent(game, false);

                RectTransform rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(130f, 130f);
                rect.localScale = Vector3.one;

                go.AddComponent<CanvasGroup>();

                Image image = go.AddComponent<Image>();
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.raycastTarget = true;
                image.color = Color.white;

                go.AddComponent<LayoutElement>();

                cloth = go.AddComponent<DraggableCloth>();
                Undo.RegisterCreatedObjectUndo(go, "EPP new item");
                created++;
            }

            List<CharacterGraphic> graphics = new List<CharacterGraphic>();
            foreach (string layerName in spec.Layers)
            {
                Transform layer = game.Find(layerName);
                CharacterGraphic graphic = layer != null ? layer.GetComponent<CharacterGraphic>() : null;
                if (graphic != null) graphics.Add(graphic);
                else report.Append("FALTA LAYER: ").Append(layerName).Append('\n');
            }

            SerializedObject serialized = new SerializedObject(cloth);
            serialized.FindProperty("clothType").enumValueIndex = (int)spec.Type;
            serialized.FindProperty("isCorrectCloth").boolValue = spec.Correct;

            SerializedProperty array = serialized.FindProperty("characterGraphics");
            array.arraySize = graphics.Count;
            for (int i = 0; i < graphics.Count; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = graphics[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();

            report.Append(spec.NewName).Append(" -> ").Append(spec.Type).Append(spec.Correct ? " CORRECTA" : " distractor")
                .Append(" layers=").Append(graphics.Count).Append('\n');
        }

        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
        EditorSceneManager.SaveScene(game.gameObject.scene);

        Debug.Log("EPPRebuildItems: creados=" + created + " re-skineados=" + reskinned + "\n" + report);
    }
}
