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

    private int currentTaskIndex = -1;
    private int score = 0;

    private List<LiquidInspectionTask> completedTask = new();

        public void OnEnable()
    {
        LiquidInspectionTask.OnTaskSelected += HandleTaskSelected;
    }

    public void OnDisaable()
    {
        LiquidInspectionTask.OnTaskSelected -= HandleTaskSelected;
    }

    private void Start()
    {
        StartTask();
    }

    private void StartTask()
    {
        if(currentTaskIndex >= tasks.Count)
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
        if (task != tasks[currentTaskIndex]) return;
        ShowVerification(task);
    }

    private void ShowVerification(LiquidInspectionTask task)
    {
        instructionsUI.ShowVerification(task, OnPlayerDecision);
    }

    private void OnPlayerDecision(LiquidInspectionTask task, bool playerSaysCorrect)
    {
        //bool success = playerSaysCorrect == task.isCorrectState;

        //if (success)
        //{
        //    score += 25;
        //}

        completedTask.Add(task);
        currentTaskIndex++;

        StartTask();
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


