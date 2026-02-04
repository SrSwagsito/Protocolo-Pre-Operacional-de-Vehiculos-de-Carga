using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LiquidLevelPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image levelBar;
    [SerializeField] private TMP_Text levelText;

    private bool isCorrectLevel;

    public Action<bool> OnPlayerDecision;

    public void ShowPanel()
    {
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
        isCorrectLevel = randomLevel >= 0.4f && randomLevel <= 0.8f;

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
