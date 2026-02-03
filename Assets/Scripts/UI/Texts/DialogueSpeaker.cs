
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

    private bool didDialogueStart;
    private bool didDialogueComplete;
    private int lineIndex;

    public float typingTime;

    private void Start()
    {
        StartDialogue();
    }

    private void Update()
    {

        //if (didDialogueComplete && !loopeable) return;

        if (Input.GetButtonDown("Fire1"))
        {
            if (!didDialogueStart)
            {
                StartDialogue();
            }

            else if (dialoguetext.text == dialogueLines[lineIndex].text)
            {
                NextDialogueLine();
            }
            else
            {
                StopAllCoroutines();
                dialoguetext.text = dialogueLines[lineIndex].text;
            }
        }
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



        OnDialogueFinished?.Invoke();
    }

    private IEnumerator ShowLine(int currentInstruction)
    {
        instructions currentLine = dialogueLines[currentInstruction];

        dialoguetext.text = string.Empty;


        foreach (char letter in currentLine .text)
        {
            dialoguetext.text += letter;

            if (currentLine.voiceClip != null && !currentLine.voiceClip.isPlaying)
            {
                currentLine.voiceClip.Play();

                Debug.Log("Playing voice clip.");
            }
            
            yield return new WaitForSeconds(typingTime);
        }

    }
}
