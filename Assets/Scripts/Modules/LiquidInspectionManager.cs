using System.Collections.Generic;
using UnityEngine;


public class LiquidInspectionManager : MonoBehaviour
{
    [SerializeField] private List<LiquidInspectionTask> tasks;
    [SerializeField] private HightlightController highlight;
    [SerializeField] private InstructionsUI instructionsUI;
    [SerializeField] private LiquidLevelPanel panelLiquid;


    private int score = 0;


    private HashSet<LiquidInspectionTask> completedTasks = new();


    private void OnEnable()
    {
        LiquidInspectionTask.OnTaskSelected += HandleTaskSelected;
        panelLiquid.OnPlayerDecision += OnLiquidPanelDecision;
    }


    private void OnDisable()
    {
        LiquidInspectionTask.OnTaskSelected -= HandleTaskSelected;
        panelLiquid.OnPlayerDecision -= OnLiquidPanelDecision;
    }


    private void Start()
    {
        panelLiquid.HidePanel();


        foreach (var task in tasks)
        {
            task.Initialize(this, panelLiquid);
        }


        instructionsUI.ShowInstruction("Selecciona un líquido para verificar.");
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
            score += 25;


        completedTasks.Add(task);


        panelLiquid.HidePanel();


        if (completedTasks.Count >= tasks.Count)
        {
            FinishModule();
            return;
        }


        instructionsUI.ShowInstruction("Selecciona otro líquido para verificar.");
        UpdateHighlights();
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
        SendResult();
    }


    private void SendResult()
    {
        ModuleResult result = new ModuleResult
        {
            moduleName = "Inspección de líquidos",
            approved = score >= 70,
            score = score,
            missedDamages = tasks.Count - completedTasks.Count,
            message = "Resultado del módulo"
        };


        GameManager.Instance.RegisterModuleResult(result);
    }
}