using UnityEngine;
using UnityEngine.UI;

public class livesui : MonoBehaviour
{
    [Header("Iconos de vida (en orden, de izquierda a derecha)")]
    [SerializeField] private Image[] lifeIcons;

    [Header("Sprites")]
    [SerializeField] private Sprite fullLifeSprite;
    [SerializeField] private Sprite emptyLifeSprite;

    private void OnEnable()
    {
        ManagerRecolection.OnLivesChanged += UpdateLives;
    }

    private void OnDisable()
    {
        ManagerRecolection.OnLivesChanged -= UpdateLives;
    }

    public void UpdateLives(int currentLives)
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] == null) continue;

            lifeIcons[i].sprite = i < currentLives ? fullLifeSprite : emptyLifeSprite;
        }
    }
}
