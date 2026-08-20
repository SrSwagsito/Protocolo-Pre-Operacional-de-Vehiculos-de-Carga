using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel de resultado del módulo de inspección de basura.
/// Se muestra cuando la inspección termina, ya sea aprobada o reprobada.
/// </summary>
public class ModuleResultResiduos : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image statusIcon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Sprites")]
    [SerializeField] private Sprite approvedIcon;
    [SerializeField] private Sprite failedIcon;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button retryButton;

    private void OnEnable()
    {
        TrashInspectionManager.OnInspectionEnded += ShowResult;
    }

    private void OnDisable()
    {
        TrashInspectionManager.OnInspectionEnded -= ShowResult;
    }

    private void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
    }

    /// <summary>
    /// Muestra el panel con el resultado del módulo
    /// </summary>
    private void ShowResult(bool approved, int score, string message)
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (statusIcon != null)
            statusIcon.sprite = approved ? approvedIcon : failedIcon;

        if (titleText != null)
            titleText.text = approved ? "¡MÓDULO APROBADO!" : "MÓDULO NO APROBADO";

        if (messageText != null)
            messageText.text = message;

        if (scoreText != null)
            scoreText.text = $"{score} pts";

        // Solo dejar avanzar si aprobó
        if (continueButton != null)
            continueButton.gameObject.SetActive(approved);

        if (retryButton != null)
            retryButton.gameObject.SetActive(!approved);
    }

    private void OnContinueClicked()
    {
        Debug.Log("Avanzando al siguiente módulo...");
        // Aquí cargas el siguiente módulo, ej:
        // SceneManager.LoadScene("SiguienteModulo");
    }

    private void OnRetryClicked()
    {
        Debug.Log("Reintentando módulo...");
        // Aquí recargas la escena actual, ej:
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}