using UnityEngine;

public class FinishDialogueButton : MonoBehaviour
{
    [SerializeField] private GameObject finishDialogueButton;

    private void OnEnable()
    {
        DialogueSpeaker.OnDialogueFinished += ShowButton;
    }
    public void OnDisable()
    {
        DialogueSpeaker.OnDialogueFinished -= ShowButton;
    }

    private void Start()
    {
       
    }

   public void ShowButton()
    {
        finishDialogueButton.SetActive(true);
        Debug.Log("Diálogo terminado, mostrando botón.");
    }
}
