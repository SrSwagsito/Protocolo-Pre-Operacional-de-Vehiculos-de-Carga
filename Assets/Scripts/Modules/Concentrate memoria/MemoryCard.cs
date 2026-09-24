using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MemoryCard : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image frontImage;
    [SerializeField] private GameObject back;

    [Header("Animación")]
    [SerializeField] private float flipDuration = 0.2f;

    private int pairID;
    private MemoryGameManager gameManager;

    private bool isRevealed = false;
    private bool isMatched = false;
    private bool canClick = true;

    public int PairID => pairID;
    public bool IsRevealed => isRevealed;
    public bool IsMatched => isMatched;

    public void Setup(int id, Sprite sprite, MemoryGameManager manager)
    {
        pairID = id;
        gameManager = manager;

        frontImage.sprite = sprite;

        isRevealed = false;
        isMatched = false;
        canClick = true;

        back.SetActive(true);
    }

    public void OnClick()
    {
        if (!canClick)
            return;

        if (isRevealed)
            return;

        if (isMatched)
            return;

        gameManager.CardSelected(this);
    }

    public void Reveal()
    {
        if (isRevealed)
            return;

        StartCoroutine(Flip(true));
    }

    public void Hide()
    {
        if (!isRevealed)
            return;

        StartCoroutine(Flip(false));
    }

    public void SetMatched()
    {
        isMatched = true;
        isRevealed = true;
        canClick = false;
    }

    public void SetInteractable(bool value)
    {
        canClick = value;
    }

    private IEnumerator Flip(bool showFront)
    {
        canClick = false;

        float elapsed = 0f;

        Vector3 originalScale = transform.localScale;

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / flipDuration;

            transform.localScale = new Vector3(
                Mathf.Lerp(1f, 0f, t),
                originalScale.y,
                originalScale.z
            );

            yield return null;
        }

        transform.localScale = new Vector3(
            0f,
            originalScale.y,
            originalScale.z
        );

        back.SetActive(!showFront);

        elapsed = 0f;

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / flipDuration;

            transform.localScale = new Vector3(
                Mathf.Lerp(0f, originalScale.x, t),
                originalScale.y,
                originalScale.z
            );

            yield return null;
        }

        transform.localScale = originalScale;

        isRevealed = showFront;

        if (!isMatched)
            canClick = true;
    }
}