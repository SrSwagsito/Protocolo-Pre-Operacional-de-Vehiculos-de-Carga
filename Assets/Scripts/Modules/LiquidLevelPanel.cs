using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

public class LiquidLevelPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image levelBar;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject panelObject ;
    [SerializeField] private LiquidInspectionTask currentTask;

    private bool isCorrectLevel;

    public Action<bool> OnPlayerDecision;

    private void Awake()
    {
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
        else { gameObject.SetActive(false); }
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

    void GenerateRandomLevel()
    {
        float randomLevel = UnityEngine.Random.Range(0f, 1f);

        levelBar.fillAmount = randomLevel;

        // Definimos rango correcto
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
