using UnityEngine;
using TMPro;

public class ProgressCounter : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private string progressFormat = "Líquidos verificados: {0}/{1}";

    public void UpdateProgress(int completed, int total)
    {
        if (progressText != null)
        {
            progressText.text = string.Format(progressFormat, completed, total);
        }
    }
}