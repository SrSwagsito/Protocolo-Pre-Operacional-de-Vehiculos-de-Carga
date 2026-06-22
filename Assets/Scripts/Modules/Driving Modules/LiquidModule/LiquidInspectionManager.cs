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
        // Resetear todo al iniciar
        ResetModule();

        LiquidInspectionTask.DisableClicks();
        Debug.Log("LiquidInspectionManager: Clics deshabilitados al inicio");

        panelLiquid.HidePanel();

        // Inicializar todas las tareas
        foreach (var task in tasks)
        {
            task.Initialize(this, panelLiquid);
        }

        // Mostrar tutorial si existe y no se ha mostrado
        if (tutorialPopup != null && !tutorialShown)
        {
            Debug.Log("LiquidInspectionManager: Mostrando tutorial");
            tutorialPopup.ShowTutorial();
            tutorialShown = true;
        }
        else
        {
            StartInspection();
        }
    }

    // Resetear todas las variables y visuales del módulo
    private void ResetModule()
    {
        Debug.Log("LiquidInspectionManager: Reseteando módulo completo");

        // Resetear variables de puntaje
        score = 0;
        correctAnswers = 0;
        completedTasks.Clear();
        tutorialShown = false; // CRÍTICO: Resetear para que vuelva a mostrar tutorial

        // Resetear clics
        LiquidInspectionTask.DisableClicks();

        // NUEVO: Resetear todas las tareas visualmente
        foreach (var task in tasks)
        {
            if (task != null)
            {
                task.ResetTask();
            }
        }

        // NUEVO: Reactivar elementos de UI si estaban desactivados
        if (instructionsUI != null && !instructionsUI.gameObject.activeSelf)
        {
            instructionsUI.gameObject.SetActive(true);
        }

        // NUEVO: Resetear contador de progreso
        if (progressCounter != null)
        {
            progressCounter.UpdateProgress(0, tasks.Count);
        }
    }

    private void StartInspection()
    {
        Debug.Log("LiquidInspectionManager: StartInspection llamado");

        if (instructionsUI != null)
            instructionsUI.ShowInstruction("Selecciona un líquido para verificar.");

        UpdateProgressCounter();

        Debug.Log("LiquidInspectionManager: EnableClicks llamado");
        LiquidInspectionTask.EnableClicks();
    }

    private void HandleTaskSelected(LiquidInspectionTask task)
    {
        Debug.Log($"LiquidInspectionManager: Tarea seleccionada - {task.vehiclePart}");

        if (completedTasks.Contains(task))
        {
            Debug.Log("LiquidInspectionManager: Esta tarea ya fue completada");
            task.ShowErrorAnimation();

            if (instructionsUI != null)
                instructionsUI.ShowInstruction("Este líquido ya fue verificado.");
            return;
        }

        if (panelLiquid.gameObject.activeSelf)
        {
            Debug.Log("LiquidInspectionManager: El panel ya está abierto, ignorando");
            return;
        }

        Debug.Log($"LiquidInspectionManager: Abriendo panel para {task.vehiclePart}");
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

        Debug.Log($"Respuesta {(success ? "correcta" : "incorrecta")}. Score: {score}");

        task.MarkAsCompleted();
        completedTasks.Add(task);
        panelLiquid.HidePanel();

        UpdateProgressCounter();

        if (completedTasks.Count >= tasks.Count)
        {
            FinishModule();
            return;
        }

        if (instructionsUI != null)
            instructionsUI.ShowInstruction("Selecciona otro líquido para verificar.");
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

        string message = $"Verificaste {correctAnswers} de {totalLiquids} líquidos correctamente.\n";

        if (score >= 75)
        {
            message += "\n¡Buen trabajo! Has demostrado competencia en la inspección de líquidos.";
        }
        else
        {
            message += $"\nTuviste {incorrect} error(es). Revisa los procedimientos de verificación de niveles.";
        }

        return message;
    }
}