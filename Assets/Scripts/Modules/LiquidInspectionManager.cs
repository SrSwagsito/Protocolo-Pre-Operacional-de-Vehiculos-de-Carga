using System.Collections.Generic;
using UnityEngine;

public class LiquidInspectionManager : MonoBehaviour
{
    [SerializeField] private List<LiquidInspectionTask> tasks;
    [SerializeField] private HightlightController highlight;
    [SerializeField] private InstructionsUI instructionsUI;
    [SerializeField] private LiquidLevelPanel panelLiquid;
    [SerializeField] private TutorialPanel tutorialPopup;
    [SerializeField] private ProgressCounter progressCounter;

    private int score = 0;
    private int correctAnswers = 0; // NUEVO: Contar respuestas correctas
    private HashSet<LiquidInspectionTask> completedTasks = new();
    private bool tutorialShown = false;

    private void OnEnable()
    {
        LiquidInspectionTask.OnTaskSelected += HandleTaskSelected;
        panelLiquid.OnPlayerDecision += OnLiquidPanelDecision;

        if (tutorialPopup != null)
            tutorialPopup.OnTutorialClosed += StartInspection;
    }

    private void OnDisable()
    {
        LiquidInspectionTask.OnTaskSelected -= HandleTaskSelected;
        panelLiquid.OnPlayerDecision -= OnLiquidPanelDecision;

        if (tutorialPopup != null)
            tutorialPopup.OnTutorialClosed -= StartInspection;
    }

    private void Start()
    {
        panelLiquid.HidePanel();

        foreach (var task in tasks)
        {
            task.Initialize(this, panelLiquid);
        }

        if (tutorialPopup != null && !tutorialShown)
        {
            tutorialPopup.ShowTutorial();
            tutorialShown = true;
        }
        else
        {
            StartInspection();
        }
    }

    private void StartInspection()
    {
        instructionsUI.ShowInstruction("Selecciona un líquido para verificar.");
        UpdateProgressCounter();
        UpdateHighlights();
    }

    private void HandleTaskSelected(LiquidInspectionTask task)
    {
        if (completedTasks.Contains(task))
        {
            instructionsUI.ShowInstruction("Este líquido ya fue verificado.");
            return;
        }

        ShowVerification(task);
    }

    private void ShowVerification(LiquidInspectionTask task)
    {
        highlight.HideHighlight();
        panelLiquid.ShowPanel(task);
    }

    private void OnLiquidPanelDecision(bool success)
    {
        var task = panelLiquid.CurrentTask;

        if (success)
        {
            score += 25;
            correctAnswers++; // NUEVO: Incrementar contador de aciertos
        }

        completedTasks.Add(task);
        panelLiquid.HidePanel();

        UpdateProgressCounter();

        if (completedTasks.Count >= tasks.Count)
        {
            FinishModule();
            return;
        }

        instructionsUI.ShowInstruction("Selecciona otro líquido para verificar.");
        UpdateHighlights();
    }

    private void UpdateProgressCounter()
    {
        if (progressCounter != null)
        {
            progressCounter.UpdateProgress(completedTasks.Count, tasks.Count);
        }
    }

    private void UpdateHighlights()
    {
        foreach (var task in tasks)
        {
            if (!completedTasks.Contains(task))
                highlight.ShowHighlight(task.transform);
        }
    }

    private void FinishModule()
    {
        // NUEVO: Ocultar elementos de inspección
        if (highlight != null)
            highlight.HideHighlight();

        if (instructionsUI != null)
            instructionsUI.gameObject.SetActive(false);

        // Enviar resultado
        SendResult();
    }

    private void SendResult()
    {
        // NUEVO: Crear mensaje detallado
        string detailMessage = GenerateDetailMessage();

        ModuleResult result = new ModuleResult
        {
            moduleName = "Inspección de Líquidos",
            approved = score >= 75, // Aprobado con 75% o más (3 de 4 correctos)
            score = score,
            missedDamages = tasks.Count - correctAnswers, // NUEVO: Líquidos mal verificados
            message = detailMessage
        };

        GameManager.Instance.RegisterModuleResult(result);
    }

    // NUEVO: Generar mensaje detallado para el panel de resultados
    private string GenerateDetailMessage()
    {
        int totalLiquids = tasks.Count;
        int incorrect = totalLiquids - correctAnswers;

        string message = $"Verificaste {correctAnswers} de {totalLiquids} líquidos correctamente.";

        if (score >= 75)
        {
            message += "¡Buen trabajo! Continua al siguiente modulo.";
        }
        else
        {
            message += $"Tuviste {incorrect} error(es). Revisa los procedimientos de verificación nuevamete.";
        }

        return message;
    }
}