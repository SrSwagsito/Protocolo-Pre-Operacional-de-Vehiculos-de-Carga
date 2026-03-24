using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// DialogueSpeaker adaptado al nuevo Input System.
/// Soporta clic/touch en PC y móvil.
/// </summary>
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

    // Control de estado del audio
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
        // Detectar input unificado (PC y móvil)
        if (WasInputDetected())
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

    /// <summary>
    /// Detecta input tanto de mouse (PC) como de touch (móvil)
    /// </summary>
    private bool WasInputDetected()
    {
        // Mouse/Click (PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        // Touch (Móvil)
        if (Touchscreen.current != null)
        {
            for (int i = 0; i < Touchscreen.current.touches.Count; i++)
            {
                if (Touchscreen.current.touches[i].press.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }

        // Teclado alternativo (Espacio o Enter)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
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

    private void UpdateSkipIndicator()
    {
        if (skipIndicator == null)
            return;

        // Mostrar indicador solo si el texto está completo y el audio terminó (o no espera audio)
        bool canSkip = CanSkipToNext();
        skipIndicator.SetActive(canSkip);
    }
}