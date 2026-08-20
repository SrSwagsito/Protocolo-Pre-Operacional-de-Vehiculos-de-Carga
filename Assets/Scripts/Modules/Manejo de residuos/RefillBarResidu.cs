using UnityEngine;
using UnityEngine.UI;

public class RefillBarResidu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image lockIcon;

    [Header("Lock States")]

    // En caso de animaciones se agrega o cambia el sprite
    [SerializeField] private Sprite openLockSprite;
    [SerializeField] private Sprite closedLockSprite;

    private int maxScore;
    private int currentScore;

    public void Initialize(int maxScore)
    {
        this.maxScore = maxScore;
        currentScore = 0;

        fillImage.fillAmount = 0f;
        lockIcon.sprite = openLockSprite;
    }

    private void OnEnable()
    {
        TrashInspectionManager.OnScoreAdded += AddScore; // antes decía ExternalInspectionManager
    }

    private void OnDisable()
    {
        TrashInspectionManager.OnScoreAdded -= AddScore;
    }

    public void AddScore(int amount)
    {
        if (maxScore <= 0)
        {
            Debug.LogError("LockProgressUI: maxScore es 0 o menor. ¿Olvidaste llamar a Initialize?");
            return;
        }


        currentScore = Mathf.Clamp(currentScore + amount, 0, maxScore);

        float fill = (float)currentScore / maxScore;
        fillImage.fillAmount = fill;

        if (currentScore >= maxScore)
        {
            lockIcon.sprite = closedLockSprite;
        }
    }
}
