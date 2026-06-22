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
    [SerializeField] private GameObject botonesPanel;

    [Header("Feedback Visual")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private GameObject correctGO;
    [SerializeField] private GameObject incorrectGO;
    [SerializeField] private float feedbackDuration = 2f;

    private LiquidState trueState;
    private LiquidInspectionTask currentTask;

    public Action<bool> OnPlayerDecision;
    public LiquidInspectionTask CurrentTask => currentTask;

    private void Awake()
    {
        // CORREGIDO: Usar GetPanelToActivate() para consistencia
        GetPanelToActivate().SetActive(false);

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        if (botonesPanel != null)
            botonesPanel.SetActive(false);
    }

    public void ShowPanel(LiquidInspectionTask task)
    {
        Debug.Log($"[ShowPanel INICIO] Task: {task.vehiclePart}");

        currentTask = task;

        // CORREGIDO: Activar el panel correcto de forma consistente
        GameObject panelToActivate = GetPanelToActivate();
        panelToActivate.SetActive(true);

        Debug.Log($"[ShowPanel] Panel activado: {panelToActivate.name}, Active: {panelToActivate.activeSelf}");

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        if (botonesPanel != null)
            botonesPanel.SetActive(false);

        GenerateRandomState();

        Debug.Log($"[ShowPanel FIN] Panel sigue activo: {panelToActivate.activeSelf}");
    }

    public void HidePanel()
    {
        GetPanelToActivate().SetActive(false);
    }

    // NUEVO: Método helper para obtener el panel correcto consistentemente
    private GameObject GetPanelToActivate()
    {
        return panelObject != null ? panelObject : gameObject;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ANIMATION EVENT
    // Llama este método desde el Animation Event al final del clip de la varilla
    // ─────────────────────────────────────────────────────────────────────────
    public void MostrarOpciones()
    {
        Debug.Log("[MostrarOpciones] Mostrando botones");

        if (botonesPanel != null)
        {
            botonesPanel.SetActive(true);
            Debug.Log($"[MostrarOpciones] Botones activados: {botonesPanel.activeSelf}");
        }
        else
        {
            Debug.LogWarning("[MostrarOpciones] botonesPanel es NULL!");
        }
    }

    private void GenerateRandomState()
    {
        int roll = UnityEngine.Random.Range(0, 4);
        levelBar.color = Color.white;
        trueState = (LiquidState)roll;

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
                levelBar.fillAmount = UnityEngine.Random.Range(0.4f, 0.85f);
                levelBar.color = new Color(0.4f, 0.2f, 0f);
                break;
        }

        levelText.text = "¿Cuál es el estado de este líquido?";

        Debug.Log($"[GenerateRandomState] Estado generado: {trueState}, Fill: {levelBar.fillAmount}");
    }

    // ── Métodos públicos para cada botón ─────────────────────────────────────

    public void OnButtonApropiado() => EvaluateAnswer(LiquidState.Correct);
    public void OnButtonNivelAbajo() => EvaluateAnswer(LiquidState.BelowLevel);
    public void OnButtonNivelArriba() => EvaluateAnswer(LiquidState.AboveLevel);
    public void OnButtonContaminado() => EvaluateAnswer(LiquidState.Contaminated);

    private void EvaluateAnswer(LiquidState playerAnswer)
    {
        Debug.Log($"[EvaluateAnswer] Jugador eligió: {playerAnswer}, Correcto era: {trueState}");

        // Ocultar botones inmediatamente
        AudioManager.Instance.Play("UI simple button");
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

            if (correctGO != null)
                AudioManager.Instance.Play("Great choose check");
                correctGO.SetActive(success);

            if (incorrectGO != null)
                AudioManager.Instance.Play("Wrong choose check");
                incorrectGO.SetActive(!success);

            yield return new WaitForSeconds(feedbackDuration);

            if (correctGO != null)
                correctGO.SetActive(false);

            if (incorrectGO != null)
                incorrectGO.SetActive(false);

            feedbackPanel.SetActive(false);
        }

        OnPlayerDecision?.Invoke(success);
        HidePanel();
    }
}