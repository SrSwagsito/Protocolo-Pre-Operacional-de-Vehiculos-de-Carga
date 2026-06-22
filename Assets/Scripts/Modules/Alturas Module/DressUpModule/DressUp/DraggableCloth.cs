using UnityEngine;

public class DraggableCloth : DraggableObject
{
    [SerializeField] private bool isCorrectCloth;

    public bool IsCorrect => isCorrectCloth;
}
