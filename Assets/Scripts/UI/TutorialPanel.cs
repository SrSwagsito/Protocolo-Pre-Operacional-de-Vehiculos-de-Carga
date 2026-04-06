using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class TutorialPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private Button closeButton;

    [Header("Animation")]
    [SerializeField] private Animator panelAnimator;
    [SerializeField] string animationTitle;

    [Header("Content")]
    [TextArea(3, 6)]
    [SerializeField] private string tutorialTitle = "Instrucciones de Inspeccion";
    [TextArea(5, 10)]
    [SerializeField]
    private string tutorialInstructions =
        "Bienvenido al modulo de inspeccion de liquidos.\n\n" +
        "• Haz clic en los puntos resaltados para revisar cada liquido\n\n" +
        "• Verifica si el nivel esta dentro del rango seguro\n\n" +
        "• Responde si el nivel es correcto o incorrecto\n\n" +
        "• Completa la inspeccion de todos los liquidos";

    public Action OnTutorialClosed;

    bool isAnimating = false;

    private void Awake()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTutorial);
    }

    public void ShowTutorial()
    {
        Debug.Log("TutorialPanel: ShowTutorial llamado");

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);


            if (titleText != null)
                titleText.text = tutorialTitle;

            if (instructionText != null)
                instructionText.text = tutorialInstructions;
            if (panelAnimator != null)
            { StartCoroutine(PlayAnimation()); }
        }
    }

    private IEnumerator PlayAnimation()
    {
        isAnimating = true;
        panelAnimator.Play(animationTitle);
        yield return null;

        float animationLength = panelAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);
        isAnimating = false;
    }



    public void CloseTutorial()
    {
        if (isAnimating) return;
        Debug.Log("TutorialPanel: CloseTutorial llamado");

        if (popupPanel != null)
            popupPanel.SetActive(false);
        AudioManager.Instance.Play("UI simple button");

        // IMPORTANTE: Invocar DESPUÉS de ocultar el panel
        // para dar tiempo a que el clic se procese
        OnTutorialClosed?.Invoke();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseTutorial);
    }
}