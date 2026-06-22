using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Pantalla final de certificación que muestra:
/// - Trofeo/medalla según el desempeño
/// - Resumen de todos los módulos completados
/// - Mensaje de felicitación personalizado
/// </summary>
public class FinalCertificationScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject certificationPanel;
    [SerializeField] private Image trophyImage;
    [SerializeField] private TextMeshProUGUI congratsText;
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private GameObject moduleResultsContainer;
    [SerializeField] private GameObject moduleResultPrefab;

    [Header("Avatar Image")]
    [SerializeField] private Image avatarImageUI;
    

    [Header("Trophy/Medal Sprites")]
    [SerializeField] private Sprite goldTrophy;
    [SerializeField] private Sprite silverTrophy;
    [SerializeField] private Sprite bronzeTrophy;
    [SerializeField] private Sprite participationMedal;

    [Header("Animation")]
    [SerializeField] private float trophyAnimationDuration = 1f;
    [SerializeField] private AnimationCurve trophyScaleCurve;

    [Header("Buttons")]
    [SerializeField] private Button finishButton;
    [SerializeField] private Button retryButton;

    private List<ModuleResult> allResults;
    private int totalScore;
    private int modulesApproved;
    private int totalModules;

    private void Start()
    {
        if (finishButton != null)
            finishButton.onClick.AddListener(OnFinishClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        // Mostrar la certificación
        ShowCertification();
    }

    /// <summary>
    /// Muestra la pantalla de certificación con todos los resultados
    /// </summary>
    public void ShowCertification()
    {
        // Obtener resultados del GameManager
        allResults = GameManager.Instance.GetAllModuleResults();

        // Calcular estadísticas
        CalculateStatistics();

        // Mostrar panel
        if (certificationPanel != null)
            certificationPanel.SetActive(true);

        // Animar y mostrar trofeo
        StartCoroutine(ShowTrophyAnimation());

        // Mostrar mensaje de felicitación
        ShowCongratsMessage();

        // Mostrar resumen general
        ShowSummary();

        // Mostrar resultados individuales
        ShowModuleResults();
    }

    /// <summary>
    /// Calcula estadísticas generales
    /// </summary>
    private void CalculateStatistics()
    {
        totalScore = 0;
        modulesApproved = 0;
        totalModules = allResults.Count;

        foreach (var result in allResults)
        {
            totalScore += result.score;
            if (result.approved)
                modulesApproved++;
        }
    }

    /// <summary>
    /// Animación del trofeo/medalla
    /// </summary>
    private IEnumerator ShowTrophyAnimation()
    {
        if (trophyImage == null) yield break;

        // Determinar qué trofeo mostrar
        Sprite trophyToShow = GetTrophySprite();
        trophyImage.sprite = trophyToShow;

        // Animación de escala
        float elapsed = 0f;
        Vector3 originalScale = trophyImage.transform.localScale;
        trophyImage.transform.localScale = Vector3.zero;

        while (elapsed < trophyAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / trophyAnimationDuration;
            float curveValue = trophyScaleCurve != null ?
                trophyScaleCurve.Evaluate(progress) : progress;

            trophyImage.transform.localScale = originalScale * curveValue;
            yield return null;
        }

        trophyImage.transform.localScale = originalScale;

        // Efecto de brillo (opcional)
        StartCoroutine(TrophyGlowEffect());
    }

    /// <summary>
    /// Determina qué trofeo mostrar según el desempeño
    /// </summary>
    private Sprite GetTrophySprite()
    {
        float approvalRate = (float)modulesApproved / totalModules;

        if (modulesApproved == totalModules && totalScore >= 3000)
        {
            // Oro: Todos aprobados con excelente puntaje
            return goldTrophy;
        }
        else if (modulesApproved == totalModules)
        {
            // Plata: Todos aprobados
            return silverTrophy;
        }
        else if (approvalRate >= 0.5f)
        {
            // Bronce: Al menos la mitad aprobados
            return bronzeTrophy;
        }
        else
        {
            // Participación: Completó el entrenamiento
            return participationMedal;
        }
    }

    /// <summary>
    /// Efecto de brillo en el trofeo
    /// </summary>
    private IEnumerator TrophyGlowEffect()
    {
        if (trophyImage == null) yield break;

        Color originalColor = trophyImage.color;
        float glowDuration = 0.5f;

        for (int i = 0; i < 2; i++)
        {
            // Brillo
            float elapsed = 0f;
            while (elapsed < glowDuration)
            {
                elapsed += Time.deltaTime;
                float brightness = Mathf.Lerp(1f, 1.3f, elapsed / glowDuration);
                trophyImage.color = originalColor * brightness;
                yield return null;
            }

            // Volver a normal
            elapsed = 0f;
            while (elapsed < glowDuration)
            {
                elapsed += Time.deltaTime;
                float brightness = Mathf.Lerp(1.3f, 1f, elapsed / glowDuration);
                trophyImage.color = originalColor * brightness;
                yield return null;
            }
        }

        trophyImage.color = originalColor;
    }

    /// <summary>
    /// Muestra mensaje de felicitación personalizado
    /// </summary>
    private void ShowCongratsMessage()
    {
        if (congratsText == null) return;

        string message = "";
        float approvalRate = (float)modulesApproved / totalModules;

        if (modulesApproved == totalModules && totalScore >= 3000)
        {
            message = "¡EXCELENTE DESEMPEÑO!\n¡Has completado tu capacitación!";
        }
        else if (modulesApproved == totalModules)
        {
            message = "¡FELICITACIONES!\n¡Has completado tu capacitación!";
        }
        else if (approvalRate >= 0.5f)
        {
            message = "¡BUEN TRABAJO!\n¡Has completado tu capacitación!";
        }
        else
        {
            message = "CAPACITACIÓN COMPLETADA\n¡Has completado tu capacitación!";
        }

        congratsText.text = message;
    }

    /// <summary>
    /// Muestra resumen general de estadísticas
    /// </summary>
    private void ShowSummary()
    {
        if (summaryText == null) return;

        string name = GameManager.Instance.UserName;
        string id = GameManager.Instance.UserID;
        Sprite avatar = GameManager.Instance.UserAvatar;

        string summary = "";
        summary += $"<color=#FFE000><b>{name.ToUpper()}</b></color>\n";
        summary += $"C.C. {id}\n\n";

        summary += $"Módulos Aprobados: {modulesApproved}\n";
        summary += $"Puntuación Total: {totalScore} pts\n";

        summaryText.text = summary;

        if (avatar != null && avatarImageUI != null)
        {
            avatarImageUI.sprite = avatar;
            avatarImageUI.gameObject.SetActive(true);
        }
        else if (avatarImageUI != null)
        {
            avatarImageUI.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Muestra los resultados individuales de cada módulo
    /// </summary>
    private void ShowModuleResults()
    {
        if (moduleResultsContainer == null || moduleResultPrefab == null) return;

        // Limpiar resultados anteriores
        foreach (Transform child in moduleResultsContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Crear resultado para cada módulo
        foreach (var result in allResults)
        {
            GameObject resultObj = Instantiate(moduleResultPrefab, moduleResultsContainer.transform);
            SetupModuleResultUI(resultObj, result);
        }
    }

    /// <summary>
    /// Configura la UI de un resultado individual
    /// </summary>
    private void SetupModuleResultUI(GameObject resultObj, ModuleResult result)
    {
        // Buscar componentes en el prefab
        TextMeshProUGUI nameText = resultObj.transform.Find("ModuleName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI scoreText = resultObj.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();
        Image statusIcon = resultObj.transform.Find("StatusIcon")?.GetComponent<Image>();

        // Configurar nombre
        if (nameText != null)
            nameText.text = result.moduleName;

        // Configurar puntaje
        if (scoreText != null)
            scoreText.text = $"{result.score} pts";

        // Configurar icono de estado
        if (statusIcon != null)
        {
            statusIcon.color = result.approved ? Color.green : Color.red;
            // Podrías cambiar el sprite también
        }
    }

    /// <summary>
    /// Botón para finalizar y salir
    /// </summary>
    private void OnFinishClicked()
    {
        // Aquí puedes:
        // - Volver al menú principal
        // - Guardar datos del usuario
        // - Salir de la aplicación

        Debug.Log("Entrenamiento completado. Saliendo...");

        // Ejemplo: Cargar escena de menú principal
        // SceneManager.LoadScene("MainMenu");

        // O cerrar la aplicación
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Botón para reintentar el entrenamiento
    /// </summary>
    private void OnRetryClicked()
    {
        // Reiniciar desde el primer módulo
        GameManager.Instance.ClearAllResults();

        // Cargar primera escena de módulo
        // SceneManager.LoadScene("LiquidInspectionModule");

        Debug.Log("Reintentando entrenamiento...");
    }
}