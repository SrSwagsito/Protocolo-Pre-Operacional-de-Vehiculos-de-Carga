using System.Collections.Generic;
using UnityEngine;

public class LiquidInspectionManager : MonoBehaviour
{
    [SerializeField] private List<LiquidInspectionTask> tasks;
    [SerializeField] private InstructionsUI instructionsUI;
    [SerializeField] private LiquidLevelPanel panelLiquid;
    [SerializeField] private TutorialPanel tutorialPopup; // Asegúrate que coincida con el nombre de tu script
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
        // Deshabilitar clics al inicio
        LiquidInspectionTask.DisableClicks();
        Debug.Log("LiquidInspectionManager: Clics deshabilitados al inicio");

        panelLiquid.HidePanel();

        foreach (var task in tasks)
        {
            task.Initialize(this, panelLiquid);
        }

        if (tutorialPopup != null && !tutorialShown)
        {
            Debug.Log("LiquidInspectionManager: Mostrando tutorial");
            tutorialPopup.ShowTutorial();
            tutorialShown = true;
        }
        else
        {
            Debug.Log("LiquidInspectionManager: No hay tutorial, iniciando directo");
            StartInspection();
        }
    }

    private void StartInspection()
    {
        Debug.Log("LiquidInspectionManager: StartInspection llamado");

        if (instructionsUI != null)
            instructionsUI.ShowInstruction("Selecciona un liquido para verificar.");

        UpdateProgressCounter();

        // Habilitar clics después del tutorial
        LiquidInspectionTask.EnableClicks();
        Debug.Log("LiquidInspectionManager: EnableClicks llamado");
    }

    private void HandleTaskSelected(LiquidInspectionTask task)
    {
        Debug.Log($"LiquidInspectionManager: Tarea seleccionada - {task.vehiclePart}");

        if (completedTasks.Contains(task))
        {
            if (instructionsUI != null)
                instructionsUI.ShowInstruction("Este liquido ya fue verificado.");
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
            Debug.Log($"Respuesta correcta. Score: {score}");
        }
        else
        {
            Debug.Log($"Respuesta incorrecta. Score: {score}");
        }

        completedTasks.Add(task);
        panelLiquid.HidePanel();

        UpdateProgressCounter();

        if (completedTasks.Count >= tasks.Count)
        {
            FinishModule();
            return;
        }

        if (instructionsUI != null)
            instructionsUI.ShowInstruction("Selecciona otro liquido para verificar.");
    }

    private void UpdateProgressCounter()
    {
        if (progressCounter != null)
        {
            progressCounter.UpdateProgress(completedTasks.Count, tasks.Count);
        }
    }

    private void FinishModule()
    {
        Debug.Log("LiquidInspectionManager: Finalizando modulo");

        // Deshabilitar clics al terminar
        LiquidInspectionTask.DisableClicks();

        if (instructionsUI != null)
            instructionsUI.gameObject.SetActive(false);

        SendResult();
    }

    private void SendResult()
    {
        string detailMessage = GenerateDetailMessage();

        ModuleResult result = new ModuleResult
        {
            moduleName = "Inspeccion de Liquidos",
            approved = score >= 75,
            score = score,
            missedDamages = tasks.Count - correctAnswers,
            message = detailMessage
        };

        Debug.Log($"Enviando resultado - Score: {score}, Aprobado: {result.approved}");
        GameManager.Instance.RegisterModuleResult(result);
    }

    private string GenerateDetailMessage()
    {
        int totalLiquids = tasks.Count;
        int incorrect = totalLiquids - correctAnswers;

        string message = $"Verificaste {correctAnswers} de {totalLiquids} liquidos correctamente.\n";

        if (score >= 75)
        {
            message += "\nBuen trabajo! Has demostrado competencia en la inspeccion de liquidos.";
        }
        else
        {
            message += $"\nTuviste {incorrect} error(es). Revisa los procedimientos de verificacion de niveles.";
        }

        return message;
    }
}