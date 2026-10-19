using System.Collections.Generic;
using UnityEngine;

public class ModuleDressUpEPPSManager : MonoBehaviour
{
    [Header("Slots disponibles")]
    [SerializeField] private List<ClothSlot> slotsAvailable;
    [SerializeField] private TutorialPanel tutorialPopUp;
    [SerializeField] private ProgressCounter progressCounter;
    [SerializeField] private InstructionsUI instructionsUI;

    [Header("Configuración")]
    [SerializeField] private int requiredCloths = 7;

    private int correctClothsSubmitted;
    private int incorrectClothsSubmitted;
    private int score;
    private bool moduleFinished;

    private void OnEnable()
    {
        if (tutorialPopUp != null) tutorialPopUp.OnTutorialClosed += StartModule;
    }

    private void OnDisable()
    {
        if (tutorialPopUp != null) tutorialPopUp.OnTutorialClosed -= StartModule;
    }

    private void Start()
    {
        if (tutorialPopUp != null) tutorialPopUp.ShowTutorial();
        else StartModule();
    }

    private void StartModule()
    {
        if (instructionsUI != null)
            instructionsUI.ShowInstruction("Arrastra los elementos de protección personal a las zonas correspondientes.");
        RefreshProgress();
    }

    public void RefreshProgress()
    {
        int correct = 0;
        if (slotsAvailable != null)
            foreach (ClothSlot slot in slotsAvailable)
            {
                DraggableCloth cloth = slot != null ? slot.GetCurrentCloth() : null;
                if (cloth != null && cloth.IsCorrect) correct++;
            }

        if (progressCounter != null)
            progressCounter.UpdateProgress(correct, Mathf.Max(1, requiredCloths));
    }

    public void FinishModule()
    {
        if (moduleFinished) return;
        correctClothsSubmitted = 0;
        incorrectClothsSubmitted = 0;

        if (slotsAvailable != null)
            foreach (ClothSlot slot in slotsAvailable)
            {
                DraggableCloth cloth = slot != null ? slot.GetCurrentCloth() : null;
                if (cloth == null) continue;
                if (cloth.IsCorrect) correctClothsSubmitted++;
                else incorrectClothsSubmitted++;
            }

        int required = Mathf.Max(1, requiredCloths);
        score = Mathf.Clamp(Mathf.RoundToInt(correctClothsSubmitted * 100f / required
                                             - incorrectClothsSubmitted * 50f / required), 0, 100);
        moduleFinished = true;
        if (instructionsUI != null) instructionsUI.gameObject.SetActive(false);
        SendResult();
    }

    private void SendResult()
    {
        ModuleResult result = new ModuleResult
        {
            moduleName = "Vestimenta de EPP",
            approved = correctClothsSubmitted >= requiredCloths && incorrectClothsSubmitted == 0,
            score = score,
            missedDamages = Mathf.Max(0, requiredCloths - correctClothsSubmitted) + incorrectClothsSubmitted,
            message = GenerateDetailMessage()
        };

        if (GameManager.Instance != null) GameManager.Instance.RegisterModuleResult(result);
        else Debug.LogError("No se encontró GameManager.Instance al finalizar Vestimenta de EPP.");
    }

    private string GenerateDetailMessage()
    {
        string message = $"Elementos correctos: {correctClothsSubmitted}/{requiredCloths}\n";
        message += $"Elementos incorrectos: {incorrectClothsSubmitted}\n\n";
        if (correctClothsSubmitted >= requiredCloths && incorrectClothsSubmitted == 0)
            message += "¡Perfecto! Identificaste todos los elementos de protección personal sin errores.";
        else if (correctClothsSubmitted >= requiredCloths)
            message += "Identificaste todos los elementos requeridos, pero incluiste algunos incorrectos.";
        else
            message += $"Te faltaron {requiredCloths - correctClothsSubmitted} elemento(s) correcto(s). Revisa el equipo de protección personal requerido.";
        return message;
    }
}
