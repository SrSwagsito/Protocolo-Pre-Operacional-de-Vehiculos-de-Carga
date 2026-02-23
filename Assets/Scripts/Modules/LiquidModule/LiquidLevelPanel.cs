using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public enum LiquidState
{
    Correct,        // Nivel apropiado 
    BelowLevel,     // Nivel por debajo
    AboveLevel,     // Nivel por encima
    Contaminated    // Líquido contaminado
}

public class LiquidLevelPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image levelBar;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject panelObject;

    [Header("Botones de Respuesta")]
    // Asigna aquí el GameObject padre que agrupe los 4 botones
    // Se oculta al inicio y aparece cuando termina la animación
    [SerializeField] private GameObject botonesPanel;

    [Header("Feedback Visual")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private GameObject correctGO;
    [SerializeField] private GameObject incorrectGO;
    [SerializeField] private float feedbackDuration = 2f;

    // Estado real generado aleatoriamente
    private LiquidState trueState;
    private LiquidInspectionTask currentTask;

    public Action<bool> OnPlayerDecision;
    public LiquidInspectionTask CurrentTask => currentTask;

    private void Awake()
    {
        if (panelObject != null)
            panelObject.SetActive(false);
        else
            gameObject.SetActive(false);

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        // Los botones siempre ocultos al arrancar
        if (botonesPanel != null)
            botonesPanel.SetActive(false);
    }

    public void ShowPanel(LiquidInspectionTask task)
    {
        currentTask = task;
        gameObject.SetActive(true);

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        // Asegurarse de que los botones estén ocultos
        // mientras corre la animación de la varilla
        if (botonesPanel != null)
            botonesPanel.SetActive(false);

        GenerateRandomState();
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ANIMATION EVENT
    // Llama este método desde el Animation Event al final del clip de la varilla.
    // Pasos en Unity:
    //   1. Abre el clip de animación en la ventana Animation
    //   2. Mueve el playhead al último frame
    //   3. Haz clic en "Add Event" (ícono de bandera)
    //   4. En el Inspector del evento, selecciona esta función: MostrarOpciones
    // ─────────────────────────────────────────────────────────────────────────
    public void MostrarOpciones()
    {
        if (botonesPanel != null)
            botonesPanel.SetActive(true);
    }

    private void GenerateRandomState()
    {
        // 25% de probabilidad para cada estado
        int roll = UnityEngine.Random.Range(0, 4);
        levelBar.color = Color.white; // Reset color por si se contaminó antes
        trueState = (LiquidState)roll;

        // Ajustar la barra visual según el estado real
        switch (trueState)
        {
            case LiquidState.Correct:
                levelBar.fillAmount = UnityEngine.Random.Range(0.2f, 0.85f);
                break;
            case LiquidState.BelowLevel:
                levelBar.fillAmount = UnityEngine.Random.Range(0.05f, 0.19f);
                break;
            case LiquidState.AboveLevel:
                levelBar.fillAmount = UnityEngine.Random.Range(0.86f, 1f);
                break;
            case LiquidState.Contaminated:
                // Nivel puede parecer correcto pero el color delata contaminación
                levelBar.fillAmount = UnityEngine.Random.Range(0.4f, 0.85f);
                levelBar.color = new Color(0.4f, 0.2f, 0f); // Café/oscuro
                break;
        }

        levelText.text = "¿Cuál es el estado de este líquido?";
    }

    // ── Métodos públicos para cada botón ─────────────────────────────────────

    public void OnButtonApropiado() => EvaluateAnswer(LiquidState.Correct);
    public void OnButtonNivelAbajo() => EvaluateAnswer(LiquidState.BelowLevel);
    public void OnButtonNivelArriba() => EvaluateAnswer(LiquidState.AboveLevel);
    public void OnButtonContaminado() => EvaluateAnswer(LiquidState.Contaminated);

    private void EvaluateAnswer(LiquidState playerAnswer)
    {
        // Ocultar botones inmediatamente para evitar doble respuesta
        if (botonesPanel != null)
            botonesPanel.SetActive(false);

        bool success = playerAnswer == trueState;
        StartCoroutine(ShowFeedbackAndClose(success));
    }

    private IEnumerator ShowFeedbackAndClose(bool success)
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);

            if (success)
                correctGO.SetActive(true);
            else
                incorrectGO.SetActive(true);

            yield return new WaitForSeconds(feedbackDuration);

            correctGO.SetActive(false);
            incorrectGO.SetActive(false);
            feedbackPanel.SetActive(false);
        }

        OnPlayerDecision?.Invoke(success);
        HidePanel();
    }
}