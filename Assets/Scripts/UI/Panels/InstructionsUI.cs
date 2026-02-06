using TMPro;
using UnityEngine;


public class InstructionsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text instructionText;


    public void ShowInstruction(string text)
    {
        instructionText.text = text;
    }
}