using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine.SocialPlatforms.Impl;
using System.Runtime.CompilerServices;

public class LiquidInspectionManager : MonoBehaviour
{
    [SerializeField] private List<LiquidInspectionTask> tasks;
    [SerializeField] private HightlightController hightlight;
    [SerializeField] private InstructionsUI instructionsUI;
    [SerializeField] private LiquidLevelPanel panelLiquid;

    private int currentTaskIndex = 0;
    private int score = 0;

    private HashSet<LiquidInspectionTask> completedTask = new();

    public void OnEnable()
    {
        LiquidInspectionTask.OnTaskSelected += HandleTaskSelected;
    }

    public void OnDisable()
    {
        LiquidInspectionTask.OnTaskSelected -= HandleTaskSelected;
    }

    private void Start()
    {

        panelLiquid.HidePanel();

        foreach (var task in tasks)
        {
            task.Initialize(this, panelLiquid);
        }
        panelLiquid.OnPlayerDecision += OnLiquidPanelDecision;

        StartTask();
    }

    private void StartTask()
    {
        if (currentTaskIndex >= tasks.Count)
        {
            FinishModule();
            return;
        }

        LiquidInspectionTask task = tasks[currentTaskIndex];

        hightlight.ShowHighlight(task.transform);

        instructionsUI.ShowInstruction("Revisa el nivel de " + task.vehiclePart);
    }

    private void HandleTaskSelected(LiquidInspectionTask task)
    {
        if (completedTask.Contains(task)) return;
        ShowVerification(task);
    }

    private void ShowVerification(LiquidInspectionTask task)
    {
        hightlight.HideHighlight();
        panelLiquid.ShowPanel(task);
        //instructionsUI.ShowVerification(task, OnPlayerDecision  );
    }

    private void OnPlayerDecision(LiquidInspectionTask task, bool playerSaysCorrect)
    {
        Debug.Log($"Decisión en UI: {playerSaysCorrect} para {task.vehiclePart}");

        panelLiquid.OnPlayerDecision(playerSaysCorrect);
    }

    public void OnLiquidPanelDecision(bool success)
    {

        if (success)
        {
            score += 25;
        }

        //completedTask.Add(panelLiquid.

        panelLiquid.HidePanel();

        currentTaskIndex++;
    }

    public void OnTaskCompleted(LiquidInspectionTask task, bool success)
    {

        currentTaskIndex++;

        if (currentTaskIndex >= tasks.Count)
        {
            // Este método ahora se llama desde otro lugar
            Debug.Log("Tarea completada: " + task.vehiclePart + " - " + (success ? "Correcto" : "Incorrecto"));
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
            moduleName = "Inspeccion de liquidos",
            approved = score >= 70,
            score = score,
            missedDamages = tasks.Count - completedTask.Count,
            message = "Resultado del modulo"
        };

        GameManager.Instance.RegisterModuleResult(result);
    }
}


