using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class LiquidLevelPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image levelBar;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject panelObject;

    [Header("Feedback Visual")] // NUEVO
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private GameObject correctGO;
    [SerializeField] private GameObject incorrectGO;
    [SerializeField] private float feedbackDuration = 2f;

    private bool isCorrectLevel;
    private LiquidInspectionTask currentTask;
    public Action<bool> OnPlayerDecision;
    public LiquidInspectionTask CurrentTask => currentTask;

    private void Awake()
    {
        if (panelObject != null)
            panelObject.SetActive(false);
        else
            gameObject.SetActive(false);

        // NUEVO: Ocultar feedback al inicio
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);
    }

    public void ShowPanel(LiquidInspectionTask task)
    {
        currentTask = task;
        gameObject.SetActive(true);

        // NUEVO: Asegurar que el feedback esté oculto
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        GenerateRandomLevel();
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    private void GenerateRandomLevel()
    {
        float randomLevel = UnityEngine.Random.Range(0f, 1f);
        levelBar.fillAmount = randomLevel;
        isCorrectLevel = randomLevel >= 0.5f && randomLevel <= 0.9f;

        if (isCorrectLevel)
            levelText.text = "¿Es el nivel adecuado?";
        else
            levelText.text = "¿Es el nivel adecuado?";
    }

    public void PlayerAnswer(bool playerSaysCorrect)
    {
        bool success = playerSaysCorrect == isCorrectLevel;

        // NUEVO: Mostrar feedback visual antes de cerrar
        StartCoroutine(ShowFeedbackAndClose(success, playerSaysCorrect));
    }

    // NUEVO: Corrutina para mostrar feedback
    private IEnumerator ShowFeedbackAndClose(bool success, bool playerSaysCorrect)
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);

            // Configurar el feedback según si acertó o no
            if (success)
            {
                correctGO.SetActive(true);
            }
            else
            {
                incorrectGO.SetActive(true);
            }

            // Esperar para que el usuario lea el feedback
            yield return new WaitForSeconds(feedbackDuration);

            correctGO.SetActive(false);
            incorrectGO.SetActive(false);
            feedbackPanel.SetActive(false);
        }

        // Notificar al manager después del feedback
        OnPlayerDecision?.Invoke(success);
        HidePanel();
    }
}