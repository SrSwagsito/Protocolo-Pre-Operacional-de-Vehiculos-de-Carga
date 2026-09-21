using System;
using UnityEngine;

[Serializable]
public class MemoryPairData
{
    [Header("Pareja")]
    public Sprite sprite;

    [Header("Preguntas")]
    public QuestionData[] questions = new QuestionData[3];
}