using System.Collections.Generic;
using UnityEngine;

public class WordSelectionController : MonoBehaviour
{
    private List<LetterCell> currentSelection =
        new List<LetterCell>();

    [SerializeField] private WordSearchManager manager;

    private bool selecting;

    //Direccion de la seleccion
    private Vector2Int direction;
    private bool directionLocked;

    public void BeginSelection(LetterCell startCell)
    {
        selecting = true;

        currentSelection.Clear();

        AddCell(startCell);
    }

    public void ContinueSelection(LetterCell cell)
    {
        if (!selecting)
            return;

        if (currentSelection.Contains(cell))
            return;

        if (directionLocked)
        {
            LetterCell last = currentSelection[currentSelection.Count - 1];

            Vector2Int expectedPos = last.GridPosition + direction;

            if (cell.GridPosition != expectedPos)
                return;
        }

        AddCell(cell);
    }

    private Vector2Int NormalizeDirection(Vector2Int dir)
    {
        return new Vector2Int(
            dir.x == 0 ? 0 : dir.x / Mathf.Abs(dir.x),
            dir.y == 0 ? 0 : dir.y / Mathf.Abs(dir.y)
        );
    }

    private void AddCell(LetterCell cell)
    {

        if (currentSelection.Contains(cell))
            return;

        if (currentSelection.Count == 1)
        {
            Vector2Int prev = currentSelection[0].GridPosition;

            direction = cell.GridPosition - prev;
            direction = NormalizeDirection(direction);

            directionLocked = true;

        }

        currentSelection.Add(cell);
        cell.Select();

        Debug.Log($"Añadida: {cell.Letter}");
    }

    private void ClearSelectionVisuals()
    {
        foreach (LetterCell cell in currentSelection)
        {
            cell.Deselect();
        }
    }

    public void EndSelection()
    {
        selecting = false;

        directionLocked = false;
        direction = Vector2Int.zero;

        string word = "";

        foreach (LetterCell cell in currentSelection)
        {
            word += cell.Letter;
        }

        Debug.Log("Palabra formada: " + word);

        bool valid = ValidateWord(word);

        if (valid)
        {
            foreach (LetterCell cell in currentSelection)
            {
                cell.MarkAsFound();
            }
        }
        else
        {
            ClearSelectionVisuals();
        }

        currentSelection.Clear();
    }

    private bool ValidateWord(string word)
    {
        word = word.ToUpper();

        foreach (string w in manager.GetWords())
        {
            string target = w.ToUpper();

            if (word == target)
            {
                if (!manager.IsWordFound(target))
                {
                    manager.RegisterFoundWord(target);
                }

                return true;
            }

            char[] reversed = target.ToCharArray();
            System.Array.Reverse(reversed);

            if (word == new string(reversed))
            {
                if (!manager.IsWordFound(target))
                {
                    manager.RegisterFoundWord(target);
                }

                return true;
            }
        }

        return false;
    }
}