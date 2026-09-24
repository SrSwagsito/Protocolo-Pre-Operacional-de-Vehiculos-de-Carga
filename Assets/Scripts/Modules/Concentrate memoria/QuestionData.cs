using System;
using UnityEngine;

public enum QuestionType
{
    MultipleChoice,
    TrueFalse
}

[Serializable]
public class QuestionData
{
    [Header("Tipo de pregunta")]
    public QuestionType questionType;

    [TextArea(2, 4)]
    public string question;

    [Header("Opciones múltiples")]
    public string optionA;
    public string optionB;
    public string optionC;
    public string optionD;

    [Header("Respuesta correcta - Opción múltiple")]
    [Range(0, 3)]
    public int correctAnswer;

    [Header("Respuesta correcta - Verdadero/Falso")]
    public bool correctTrueFalse;
}