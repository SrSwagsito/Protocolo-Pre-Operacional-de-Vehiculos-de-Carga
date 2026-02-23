using System.Collections.Generic;
using UnityEngine;

public class LiquidInspectionManager : MonoBehaviour
{
    [SerializeField] private List<LiquidInspectionTask> tasks;
    [SerializeField] private InstructionsUI instructionsUI;
    [SerializeField] private LiquidLevelPanel panelLiquid;
    [SerializeField] private TutorialPanel tutorialPopup;
    [SerializeField] private ProgressCounter progressCounter;

    private int score = 0;
    private int correctAnswers = 0;
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
    }

    private void HandleTaskSelected(LiquidInspectionTask task)
    {
        if (completedTasks.Contains(task))
        {
            instructionsUI.ShowInstruction("Este líquido ya fue verificado.");
            return;
        }

        panelLiquid.ShowPanel(task);
    }

    private void OnLiquidPanelDecision(bool success)
    {
        var task = panelLiquid.CurrentTask;

        if (success)
        {
            score += 25;
            correctAnswers++;
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
    }

    private void UpdateProgressCounter()
    {
        if (progressCounter != null)
            progressCounter.UpdateProgress(completedTasks.Count, tasks.Count);
    }

    public void FinishModule()
    {
        if (instructionsUI != null)
            instructionsUI.gameObject.SetActive(false);

        SendResult();
    }

    private void SendResult()
    {
        string detailMessage = GenerateDetailMessage();

        ModuleResult result = new ModuleResult
        {
            moduleName = "Inspección de Líquidos",
            approved = score >= 75,
            score = score,
            missedDamages = tasks.Count - correctAnswers,
            message = detailMessage
        };

        GameManager.Instance.RegisterModuleResult(result);
    }

    private string GenerateDetailMessage()
    {
        int totalLiquids = tasks.Count;
        int incorrect = totalLiquids - correctAnswers;

        string message = $"Verificaste {correctAnswers} de {totalLiquids} líquidos correctamente. ";

        if (score >= 75)
            message += "¡Buen trabajo! Continúa al siguiente módulo.";
        else
            message += $"Tuviste {incorrect} error(es). Revisa los procedimientos de verificación nuevamente.";

        return message;
    }
}