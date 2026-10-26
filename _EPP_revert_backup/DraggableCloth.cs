using UnityEngine;

public class DraggableCloth : DraggableObject
{
    public enum ClothType
    {
        Helmet,
        Glasses,
        Harness,
        Shirt,
        Gloves,
        Pants,
        Boots
    }

    [Header("Cloth")]

    [SerializeField]
    private ClothType clothType;

    [SerializeField]
    private bool isCorrectCloth;

    [Header("Character Graphics")]

    [SerializeField]
    private CharacterGraphic[] characterGraphics;

    public ClothType Cloth => clothType;

    public bool IsCorrect => isCorrectCloth;

    public CharacterGraphic[] CharacterGraphics => characterGraphics;
}
