using TMPro;
using UnityEngine;

public class ProgressCounterTrash : MonoBehaviour
{
    [SerializeField] private TMP_Text progressText;

    public void UpdateProgress(int completed, int total)
    {
        if (progressText == null) return;

        progressText.text =
            $"Residuos clasificados: {completed}/{total}";
    }
}