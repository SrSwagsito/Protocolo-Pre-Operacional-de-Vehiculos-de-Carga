using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueSpeaker : MonoBehaviour
{
    [System.Serializable]
    public class instructions
    {
        [TextArea(3, 5)]
        public string text;
        public AudioSource voiceClip;
    }

    public static event Action OnDialogueFinished;

    [SerializeField] public GameObject dialoguePanel;
    [SerializeField] public TMP_Text dialoguetext;
    [SerializeField] private List<instructions> dialogueLines = new List<instructions>();
    [SerializeField] private bool loopeable;

    [Header("Audio Settings")]
    [SerializeField] private bool waitForAudioToFinish = true;
    [SerializeField] private GameObject skipIndicator;

    private bool didDialogueStart;
    private bool didDialogueComplete;
    private int lineIndex;
    public float typingTime;

    // NUEVO: Control de estado del audio
    private bool isAudioPlaying = false;
    private bool isTyping = false;

    private void Start()
    {
        if (skipIndicator != null)
            skipIndicator.SetActive(false);

        StartDialogue();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (!didDialogueStart)
            {
                StartDialogue();
            }
            else if (isTyping)
            {
                // Si está escribiendo, completar el texto inmediatamente
                StopAllCoroutines();
                dialoguetext.text = dialogueLines[lineIndex].text;
                isTyping = false;

                // Mostrar indicador de skip si el audio aún está sonando
                UpdateSkipIndicator();
            }
            else if (CanSkipToNext())
            {
                // Solo puede avanzar si el audio terminó (o no hay audio)
                NextDialogueLine();
            }
        }

        UpdateAudioState();
    }

    public void StartDialogue()
    {
        if (didDialogueComplete && !loopeable) return;

        didDialogueStart = true;
        dialoguePanel.SetActive(true);
        lineIndex = 0;
        StartCoroutine(ShowLine(lineIndex));
    }

    public void NextDialogueLine()
    {
        lineIndex++;

        if (skipIndicator != null)
            skipIndicator.SetActive(false);

        if (lineIndex < dialogueLines.Count)
        {
            StartCoroutine(ShowLine(lineIndex));
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        Debug.Log("Diálogo terminado.");
        didDialogueComplete = true;
        dialoguePanel.SetActive(false);
        didDialogueStart = false;

        if (skipIndicator != null)
            skipIndicator.SetActive(false);

        OnDialogueFinished?.Invoke();
    }

    private IEnumerator ShowLine(int currentInstruction)
    {
        instructions currentLine = dialogueLines[currentInstruction];
        dialoguetext.text = string.Empty;
        isTyping = true;

        // Reproducir el audio al inicio del diálogo
        if (currentLine.voiceClip != null)
        {
            currentLine.voiceClip.Play();
            isAudioPlaying = true;
            Debug.Log("Playing voice clip.");
        }

        // Escribir letra por letra
        foreach (char letter in currentLine.text)
        {
            dialoguetext.text += letter;
            yield return new WaitForSeconds(typingTime);
        }

        isTyping = false;

        // Mostrar indicador de que puede continuar
        UpdateSkipIndicator();
    }

    // NUEVO: Verificar si se puede avanzar al siguiente diálogo
    private bool CanSkipToNext()
    {
        // Si el texto no está completo, no se puede avanzar
        if (dialoguetext.text != dialogueLines[lineIndex].text)
            return false;

        // Si está configurado para esperar el audio y el audio está sonando, no se puede avanzar
        if (waitForAudioToFinish && isAudioPlaying)
            return false;

        return true;
    }

    // NUEVO: Actualizar el estado del audio
    private void UpdateAudioState()
    {
        if (isAudioPlaying && lineIndex < dialogueLines.Count)
        {
            AudioSource currentAudio = dialogueLines[lineIndex].voiceClip;

            if (currentAudio != null && !currentAudio.isPlaying)
            {
                isAudioPlaying = false;
                UpdateSkipIndicator();
            }
        }
    }

    // NUEVO: Mostrar/ocultar indicador de skip
    private void UpdateSkipIndicator()
    {
        if (skipIndicator == null)
            return;

        // Mostrar indicador solo si el texto está completo y el audio terminó (o no espera audio)
        bool canSkip = CanSkipToNext();
        skipIndicator.SetActive(canSkip);
    }
}