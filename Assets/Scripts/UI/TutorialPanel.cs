using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TutorialPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private Button closeButton;

    [Header("Content")]
    [TextArea(3, 6)]
    [SerializeField] private string tutorialTitle = "Instrucciones de Inspección";
    [TextArea(5, 10)]
    [SerializeField] private string tutorialInstructions;

    public Action OnTutorialClosed;

    private void Awake()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTutorial);
    }

    public void ShowTutorial()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);

            if (titleText != null)
                titleText.text = tutorialTitle;

            if (instructionText != null)
                instructionText.text = tutorialInstructions;
        }
    }

    public void CloseTutorial()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        OnTutorialClosed?.Invoke();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseTutorial);
    }
}