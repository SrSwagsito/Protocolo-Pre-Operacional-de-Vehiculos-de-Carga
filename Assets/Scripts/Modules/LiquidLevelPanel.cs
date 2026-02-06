using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class LiquidLevelPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image levelBar;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject panelObject;


    private bool isCorrectLevel;
    private LiquidInspectionTask currentTask;


    public Action<bool> OnPlayerDecision;


    public LiquidInspectionTask CurrentTask => currentTask;


    private void Awake()
    {
        if (panelObject != null)
            panelObject.SetActive(false);
        else
            gameObject.SetActive(false);
    }


    public void ShowPanel(LiquidInspectionTask task)
    {
        currentTask = task;
        gameObject.SetActive(true);
        GenerateRandomLevel();
    }


    public void HidePanel()
    {
        gameObject.SetActive(false);
    }


    private void GenerateRandomLevel()
    {
        float randomLevel = UnityEngine.Random.Range(0f, 1f);


        levelBar.fillAmount = randomLevel;


        isCorrectLevel = randomLevel >= 0.5f && randomLevel <= 0.9f;


        if (isCorrectLevel)
            levelText.text = "Nivel dentro del rango seguro";
        else
            levelText.text = "Nivel fuera del rango seguro";
    }


    public void PlayerAnswer(bool playerSaysCorrect)
    {
        bool success = playerSaysCorrect == isCorrectLevel;


        OnPlayerDecision?.Invoke(success);
        HidePanel();
    }
}