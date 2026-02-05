using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsUI : MonoBehaviour
{
    [SerializeField] private GameObject verificationPanel;
    [SerializeField] private TMP_Text instructionText;

    [SerializeField] private Button correctButton;
    [SerializeField] private Button wrongButton;

    private LiquidInspectionTask currentTask;
    private Action<LiquidInspectionTask, bool> callback;

    public void ShowInstruction(string text)
    {
        instructionText.text = text;
        //verificationPanel.SetActive(false);
    }

    public void ShowVerification(LiquidInspectionTask task, Action<LiquidInspectionTask, bool> onDecision)
    {
        currentTask = task;
        callback = onDecision;

        verificationPanel.SetActive(true);

        correctButton.onClick.RemoveAllListeners();
        wrongButton.onClick.RemoveAllListeners();

        correctButton.onClick.AddListener(() => Decide(true));
        wrongButton.onClick.AddListener(() => Decide(false));
    }

    private void Decide(bool playerChoice)
    {
        verificationPanel.SetActive(false);
        callback?.Invoke(currentTask, playerChoice);
    }
}
