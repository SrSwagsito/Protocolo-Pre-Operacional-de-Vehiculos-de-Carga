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
    [SerializeField] private int requiredCloths; // Prendas necesarias

    private int correctClothsSubmitted = 0;
    private int incorrectClothsSubmitted = 0;
    private int score = 0;
    private bool moduleFinished = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        if(tutorialPopUp != null)
        tutorialPopUp.OnTutorialClosed += StartModule;


    }

    private void OnDisable()
    {
        if(tutorialPopUp != null) 
        tutorialPopUp.OnTutorialClosed -= StartModule;

    }
    void Start()
    {
        if (tutorialPopUp != null)
        {
            tutorialPopUp.ShowTutorial();
        }
        else
        {
            StartModule();
        }
    }

    

    private void StartModule()
    {
        instructionsUI.ShowInstruction("Arrastra los documentos necesarios a la zona de verificación.");
        UpdateProgressCounter();
    }

    public void FinishModule()
    {
        if (moduleFinished) return;

        correctClothsSubmitted = 0;
        incorrectClothsSubmitted = 0;
        score = 0;

        // Recorremos todos los slots
        foreach (var slot in slotsAvailable)
        {
            DraggableCloth cloth = (DraggableCloth)slot.GetCurrentCloth();
            if (cloth == null)
            {
                Debug.Log($"Slot {slot.name} no contiene una DraggableCloth válida.");
                continue;
            }

            // Verificamos si la prenda es la correcta
            if (cloth.IsCorrect) // propiedad booleana en DraggableObject
            {
                correctClothsSubmitted++;
                score += 20;
                Debug.Log($"✓ Correcto: {cloth.name}");
            }
            else
            {
                incorrectClothsSubmitted++;
                score -= 10;
                Debug.Log($"✗ Incorrecto: {cloth.name}");
            }
        }

        moduleFinished = true;

        // Ocultar instrucciones
        if (instructionsUI != null)
            instructionsUI.gameObject.SetActive(false);

        // Asegurar que el score no sea negativo
        if (score < 0)
            score = 0;

        SendResult();
    }

    private void UpdateProgressCounter()
    {
        if (progressCounter != null)
        {
            progressCounter.UpdateProgress(correctClothsSubmitted, requiredCloths);
        }
    }

    

    private void SendResult()
    {
        string detailMessage = GenerateDetailMessage();

        ModuleResult result = new ModuleResult
        {
            moduleName = "Vestimenta de EPPS",
            approved = correctClothsSubmitted >= requiredCloths && incorrectClothsSubmitted == 0,
            score = score,
            missedDamages = requiredCloths - correctClothsSubmitted + incorrectClothsSubmitted,
            message = detailMessage
        };

        GameManager.Instance.RegisterModuleResult(result);
    }

    private string GenerateDetailMessage()
    {
        string message = $"Documentos correctos: {correctClothsSubmitted}/{requiredCloths}\n";
        message += $"Documentos incorrectos añadidos: {incorrectClothsSubmitted}\n\n";

        if (correctClothsSubmitted >= requiredCloths && incorrectClothsSubmitted == 0)
        {
            message += "¡Perfecto! Identificaste todos los documentos necesarios sin errores.";
        }
        else if (correctClothsSubmitted >= requiredCloths)
        {
            message += "Identificaste todos los documentos, pero incluiste algunos innecesarios.";
        }
        else
        {
            int missing = requiredCloths - correctClothsSubmitted;
            message += $"Te faltaron {missing} documento(s) necesario(s). Revisa la lista de documentación requerida.";
        }

        return message;
    }
}


