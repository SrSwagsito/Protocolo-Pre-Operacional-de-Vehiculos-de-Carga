using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LetterCell : MonoBehaviour,
    IPointerEnterHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private TMP_Text letterText;
    [SerializeField] private Image background;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color foundColor = Color.green;

    public Vector2Int GridPosition { get; private set; }
    public char Letter { get; private set; }

    bool IsFound;

    private WordSelectionController selectionController;

    public void Initialize(char letter, Vector2Int pos, WordSelectionController controller)
    {
        Letter = letter;
        GridPosition = pos;
        selectionController = controller;

        letterText.text = letter.ToString();
        Deselect();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        selectionController.BeginSelection(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        selectionController.ContinueSelection(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        selectionController.EndSelection();
    }

    public void Select()
    {
        background.color = selectedColor;
    }

    public void Deselect()
    {
        if (IsFound) return;
    background.color = normalColor;
    }

    public void MarkAsFound()
    {
        IsFound = true;
        background.color = foundColor;
    }
}