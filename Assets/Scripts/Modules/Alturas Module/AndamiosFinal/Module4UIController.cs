using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Gestiona toda la UI del Módulo 4.
/// 
/// Setup en Inspector:
///   - stackLivesContainer / ropeLivesContainer : Transform padre donde
///     se instancian los iconos de vida (ej. un HorizontalLayoutGroup).
///   - lifeIconPrefab : prefab con un componente Image (corazón / casco, etc.).
///   - stackPanel / ropePanel : paneles raíz de cada fase (se activan/desactivan).
/// 
/// Los iconos activos se muestran con alpha 1; los perdidos con alpha 0.3.
/// </summary>
public class Module4UIController : MonoBehaviour
{
    [Header("Fase 1 — Vidas Stack")]
    public Transform stackLivesContainer;

    [Header("Fase 2 — Vidas Rapel")]
    public Transform ropeLivesContainer;

    [Header("Prefab de icono de vida")]
    public GameObject lifeIconPrefab;

    [Header("Paneles de fase")]
    public GameObject stackPanel;
    public GameObject ropePanel;

    // ─── Colores de estado ────────────────────────────────────────────────────
    private static readonly Color ActiveColor = Color.white;
    private static readonly Color LostColor   = new Color(1f, 1f, 1f, 0.25f);

    private List<Image> _stackIcons = new();
    private List<Image> _ropeIcons  = new();

    // ─── Inicialización ───────────────────────────────────────────────────────

    /// <summary>Llamar desde Module4Manager antes de StartStacking().</summary>
    public void InitializeStackLives(int maxLives)
    {
        _stackIcons = BuildIcons(stackLivesContainer, maxLives);
    }

    /// <summary>Llamar desde Module4Manager antes de StartRopePhase().</summary>
    public void InitializeRopeLives(int maxLives)
    {
        _ropeIcons = BuildIcons(ropeLivesContainer, maxLives);
    }

    // ─── Actualización de vidas ───────────────────────────────────────────────

    /// <summary>Recibe (current, max) desde LifeSystem.OnLivesChanged.</summary>
    public void UpdateStackLives(int current, int max) => RefreshIcons(_stackIcons, current);

    /// <summary>Recibe (current, max) desde LifeSystem.OnLivesChanged.</summary>
    public void UpdateRopeLives(int current, int max)  => RefreshIcons(_ropeIcons,  current);

    // ─── Cambio de panel ──────────────────────────────────────────────────────

    public void ShowStackPanel()
    {
        stackPanel?.SetActive(true);
        ropePanel?.SetActive(false);
    }

    public void ShowRopePanel()
    {
        stackPanel?.SetActive(false);
        ropePanel?.SetActive(true);
    }

    // ─── Helpers privados ─────────────────────────────────────────────────────

    private List<Image> BuildIcons(Transform container, int count)
    {
        var list = new List<Image>();
        if (container == null || lifeIconPrefab == null)
        {
            Debug.LogWarning("Module4UIController: container o lifeIconPrefab no asignado.");
            return list;
        }

        // Limpiar iconos anteriores (útil si se reinicia el módulo)
        foreach (Transform child in container)
            Destroy(child.gameObject);

        for (int i = 0; i < count; i++)
        {
            var go  = Instantiate(lifeIconPrefab, container);
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                img.color = ActiveColor;
                list.Add(img);
            }
        }
        return list;
    }

    private static void RefreshIcons(List<Image> icons, int current)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            if (icons[i] == null) continue;
            icons[i].color = i < current ? ActiveColor : LostColor;
        }
    }
}
