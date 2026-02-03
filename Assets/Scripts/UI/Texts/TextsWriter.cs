using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextsWriter : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 5)]
        public string text;
        public float typingSpeed = 30f;
        public AudioClip voiceClip; 
        public GameObject speaker;  
    }

    [Header("Referencias")]
    public TMP_Text dialogueText;
    public GameObject continuePrompt;
    [SerializeField] AudioSource voice;


    [Header("Configuración")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    public bool autoStart = true;
    [SerializeField] private bool loopeable;
    private bool dialogueCompleted = false;


    // Eventos
    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action<int> OnLineStart; // índice de la línea
    public event Action<int> OnLineEnd;

    private int currentLineIndex = -1;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (autoStart)
        {
            StartDialogue();
        }

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }
    }

    void Update()
    {
        // Continuar con click/tecla espacio
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                SkipCurrentLine();
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue()
    {
        if (dialogueCompleted && !loopeable) return;

        if (dialogueLines.Count == 0)
        {
            Debug.LogWarning("No hay líneas de diálogo configuradas");
            return;
        }

        dialogueCompleted = false;
        currentLineIndex = -1;

        OnDialogueStart?.Invoke();
        NextLine();
    }

    public void NextLine()
    {
        if (isTyping) return;

        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

      typingCoroutine = StartCoroutine(TypeLine(currentLineIndex));
    }

    IEnumerator TypeLine(int lineIndex)
    {
        DialogueLine line = dialogueLines[lineIndex];

        // Preparar
        dialogueText.text = "";
        isTyping = true;

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        OnLineStart?.Invoke(lineIndex);

        // Escribir caracter por caracter
        for (int i = 0; i < line.text.Length; i++)
        {   
            dialogueText.text += line.text[i];

            // Reproducir sonido de voz si existe
            //if (line.voiceClip != null && i % 3 == 0 && voice != null) // Cada 3 caracteres
            //{
            //    voice.PlayOneShot(line.voiceClip, 2f);
            //}

            yield return new WaitForSeconds(1f / line.typingSpeed);
        }

        // Línea completada
        isTyping = false;
        OnLineEnd?.Invoke(lineIndex);

        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    void SkipCurrentLine()
    {
        if (!isTyping) return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        DialogueLine line = dialogueLines[currentLineIndex];
        dialogueText.text = line.text;
        isTyping = false;

        OnLineEnd?.Invoke(currentLineIndex);

        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    void EndDialogue()
    {
        dialogueText.text = "";
        dialogueCompleted = true;

        OnDialogueEnd?.Invoke();

        if (continuePrompt != null)
            continuePrompt.SetActive(false);


    }

    // Métodos públicos para control externo
    public void AddLine(string text, float speed = 30f)
    {
        dialogueLines.Add(new DialogueLine
        {
            text = text,
            typingSpeed = speed
        });
    }

    public void ClearDialogue()
    {
        dialogueLines.Clear();
        currentLineIndex = -1;
        dialogueText.text = "";
        isTyping = false;
    }

    public bool IsDialogueActive => currentLineIndex >= 0;
    public bool IsTyping => isTyping;
    public int CurrentLine => currentLineIndex;
    public int TotalLines => dialogueLines.Count;
}