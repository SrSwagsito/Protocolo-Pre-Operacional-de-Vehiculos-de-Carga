using System.Collections.Generic;
using UnityEngine;

public class DocumentCheckerManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private List<DraggableDocument> availableDocuments;
    [SerializeField] private DocumentDropZone dropZone;
    [SerializeField] private TutorialPanel tutorialPopup;
    [SerializeField] private ProgressCounter progressCounter;
    [SerializeField] private InstructionsUI instructionsUI;

    [Header("Configuración")]
    [SerializeField] private int requiredDocuments; // Documentos correctos necesarios

    private int correctDocumentsSubmitted = 0;
    private int incorrectDocumentsSubmitted = 0;
    private int score = 0;
    private bool moduleFinished = false;

    private void OnEnable()
    {
        dropZone.OnDocumentDropped += HandleDocumentDropped;

        if (tutorialPopup != null)
            tutorialPopup.OnTutorialClosed += StartInspection;
    }

    private void OnDisable()
    {
        dropZone.OnDocumentDropped -= HandleDocumentDropped;

        if (tutorialPopup != null)
            tutorialPopup.OnTutorialClosed -= StartInspection;
    }

    private void Start()
    {
        // Inicializar todos los documentos
        foreach (var doc in availableDocuments)
        {
            doc.Initialize(this);
        }

        // Mostrar tutorial
        if (tutorialPopup != null)
        {
            tutorialPopup.ShowTutorial();
        }
        else
        {
            StartInspection();
        }
    }

    private void StartInspection()
    {
        instructionsUI.ShowInstruction("Arrastra los documentos necesarios a la zona de verificación.");
        UpdateProgressCounter();
    }

    private void HandleDocumentDropped(DraggableDocument document)
    {
        if (moduleFinished)
            return;

        // Verificar si el documento es correcto
        if (document.IsRequired)
        {
            correctDocumentsSubmitted++;
            score += 20; // 100 puntos / 5 documentos = 20 por correcto
            //instructionsUI.ShowInstruction($"✓ Correcto: {document.DocumentName}");
        }
        else
        {
            incorrectDocumentsSubmitted++;
            score -= 10; // Penalización por documento incorrecto
            //instructionsUI.ShowInstruction($"✗ Este documento NO es necesario: {document.DocumentName}");
        }

        // Eliminar el documento de la lista visual
        document.gameObject.SetActive(false);

        UpdateProgressCounter();

        // Verificar si ya se completó el módulo
        if (correctDocumentsSubmitted >= requiredDocuments)
        {
            FinishModule();
        }
        else if (availableDocuments.TrueForAll(doc => !doc.gameObject.activeSelf))  
        {
            // Si se agotaron todos los documentos sin completar
            FinishModule();
        }
        else
        {
            instructionsUI.ShowInstruction("Continúa seleccionando los documentos necesarios.");
        }
    }

    private void UpdateProgressCounter()
    {
        if (progressCounter != null)
        {
            progressCounter.UpdateProgress(correctDocumentsSubmitted, requiredDocuments);
        }
    }

    private void FinishModule()
    {
        moduleFinished = true;

        // Ocultar instrucciones
        if (instructionsUI != null)
            instructionsUI.gameObject.SetActive(false);

        // Asegurar que el score no sea negativo
        if (score < 0)
            score = 0;

        SendResult();
    }

    private void SendResult()
    {
        string detailMessage = GenerateDetailMessage();

        ModuleResult result = new ModuleResult
        {
            moduleName = "Inspección de Documentos",
            approved = correctDocumentsSubmitted >= requiredDocuments && incorrectDocumentsSubmitted == 0,
            score = score,
            missedDamages = requiredDocuments - correctDocumentsSubmitted + incorrectDocumentsSubmitted,
            message = detailMessage
        };

        GameManager.Instance.RegisterModuleResult(result);
    }

    private string GenerateDetailMessage()
    {
        string message = $"Documentos correctos: {correctDocumentsSubmitted}/{requiredDocuments}\n";
        message += $"Documentos incorrectos añadidos: {incorrectDocumentsSubmitted}\n\n";

        if (correctDocumentsSubmitted >= requiredDocuments && incorrectDocumentsSubmitted == 0)
        {
            message += "¡Perfecto! Identificaste todos los documentos necesarios sin errores.";
        }
        else if (correctDocumentsSubmitted >= requiredDocuments)
        {
            message += "Identificaste todos los documentos, pero incluiste algunos innecesarios.";
        }
        else
        {
            int missing = requiredDocuments - correctDocumentsSubmitted;
            message += $"Te faltaron {missing} documento(s) necesario(s). Revisa la lista de documentación requerida.";
        }

        return message;
    }
}