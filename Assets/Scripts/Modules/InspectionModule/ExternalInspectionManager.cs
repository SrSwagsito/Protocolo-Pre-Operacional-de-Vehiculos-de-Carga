//using NUnit.Framework.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class ExternalInspectionManager : MonoBehaviour
//{
//    public static Action<int> OnScoreAdded;

//    [SerializeField] private Refillbar bar;



//    public int score = 0;

//    private List<DamagedZone> realDamages = new List<DamagedZone>();
//    private List<DamagedZone> reportedDamages = new List<DamagedZone>();

//    //public int wrongReports = 0;
//    private int totalDamageParts;

//    private string resultMessage;
//    private bool moduleApproved;

//    public void Start()
//    {
//        if (bar == null)
//        {
//            return;
//        }

//        else {
//            bar.Initialize(100); }// puntaje máximo
//        }

//    public void RegisterRealDamage(DamagedZone damage)
//    {
//        if (!realDamages.Contains(damage))
//            realDamages.Add(damage);
//    }

//    public void RegisterCorrectDamage(DamagedZone damage)
//    {
//        if (!reportedDamages.Contains(damage))
//        {
//            reportedDamages.Add(damage);
//            score += 10;
//            Debug.Log("Daño informado correctamente");

//            OnScoreAdded?.Invoke(10);
//        }
//    }



//    public void RegisterWrongReport()
//    {
//        //wrongReports++;
//        //score -= 15;
//        Debug.Log("Reporte incorrecto");
//    }

//    public void Dispatch()
//    {
//        // Daños no detectados
//        //int missedDamages = realDamages.Count - reportedDamages.Count;

//        //if (missedDamages > 0)
//        //{
//        //    score -= missedDamages * 25;
//        //    Debug.Log("No se detectaron todos los daños");
//        //}

//        EvaluateResult();
//    }

//    public void SetTotalDamage(int value)
//    {
//        totalDamageParts = value;
//    }

//    private void EvaluateResult()
//    {
//        int missedReports = totalDamageParts - reportedDamages.Count;



//        Debug.Log("Puntaje final: " + score);

//        if (score >= 70)
//            ApprovedModule(missedReports);
//        else
//            FailedModule(missedReports);

//        SendResult();
//    }

//    private void ApprovedModule(int missedReports)
//    {

//        moduleApproved = true;

//        if (missedReports > 0)
//        {
//            resultMessage = "Haz aprobado, sin embargo, has pasado por alto " + missedReports + " observaciones. Recuerda que tu seguridad es primero, procura que siempre tus probabilidades de sufrir un accidente sean cero.";
//        }
//        else

//            resultMessage = "Felicitaciones, haz realizado una inspeccion eficaz.";
//    }

//    private void FailedModule(int missedReports)
//    {
//        moduleApproved = false;
//        resultMessage = "El vehiculo es demasiado peligroso para estar operativo. No has realizado la inspeccion correctamente.";
//    }

//    private void SendResult()
//    {
//        ModuleResult result = new ModuleResult
//        {
//            moduleName = "Inspección Externa",
//            approved = moduleApproved,
//            score = score,
//            missedDamages = totalDamageParts - reportedDamages.Count,
//            message = resultMessage
//        };

//        GameManager.Instance.RegisterModuleResult(result);
//    }
//}

using System;
using System.Collections.Generic;
using UnityEngine;

public class ExternalInspectionManager : MonoBehaviour
{
    public static Action<int> OnScoreAdded;

    [Header("UI References")]
    [SerializeField] private Refillbar bar;
    [SerializeField] private TutorialPanel tutorialPopup; // NUEVO - Opcional
    [SerializeField] private ProgressCounter progressCounter; // NUEVO - Opcional
    [SerializeField] private InstructionsUI instructionsUI; // NUEVO - Opcional

    [Header("Inspection Settings")]
    public int score = 0;
    private List<DamagedZone> realDamages = new List<DamagedZone>();
    private List<DamagedZone> reportedDamages = new List<DamagedZone>();
    private int totalDamageParts;
    private string resultMessage;
    private bool moduleApproved;
    private bool tutorialShown = false; // NUEVO

    private void OnEnable()
    {
        // NUEVO: Suscribirse al evento del tutorial si existe
        if (tutorialPopup != null)
        {
            tutorialPopup.OnTutorialClosed += OnTutorialClosed;
        }
    }

    private void OnDisable()
    {
        // NUEVO: Desuscribirse del evento
        if (tutorialPopup != null)
        {
            tutorialPopup.OnTutorialClosed -= OnTutorialClosed;
        }
    }

    public void Start()
    {
        // Inicializar la barra si existe
        if (bar != null)
        {
            bar.Initialize(100); // puntaje máximo
        }

        // NUEVO: Mostrar tutorial al inicio si existe y no se ha mostrado
        if (tutorialPopup != null && !tutorialShown)
        {
            tutorialPopup.ShowTutorial();
            tutorialShown = true;
        }
        else
        {
            // Si no hay tutorial, iniciar directamente
            StartInspection();
        }
    }

    // NUEVO: Se llama cuando se cierra el tutorial
    private void OnTutorialClosed()
    {
        StartInspection();
    }

    // NUEVO: Iniciar la inspección después del tutorial
    private void StartInspection()
    {
        // Mostrar instrucciones iniciales si existe el componente
        if (instructionsUI != null)
        {
            instructionsUI.ShowInstruction("Inspecciona el vehículo y reporta los daños que encuentres.");
        }

        // Actualizar contador de progreso si existe
        UpdateProgressCounter();

        Debug.Log("Inspección iniciada");
    }

    public void RegisterRealDamage(DamagedZone damage)
    {
        if (!realDamages.Contains(damage))
            realDamages.Add(damage);
    }

    public void RegisterCorrectDamage(DamagedZone damage)
    {
        if (!reportedDamages.Contains(damage))
        {

            reportedDamages.Add(damage);
            score += 10;
            AudioManager.Instance.Play("UI simple button");
            Debug.Log("Daño informado correctamente");
            OnScoreAdded?.Invoke(10);

            // NUEVO: Actualizar contador de progreso
            UpdateProgressCounter();

            // NUEVO: Actualizar instrucciones
            if (instructionsUI != null)
            {
                instructionsUI.ShowInstruction($"Daño reportado. Progreso: {reportedDamages.Count}/{totalDamageParts}");
            }
        }
    }

    public void RegisterWrongReport()
    {
        Debug.Log("Reporte incorrecto");

        // NUEVO: Mostrar feedback de error
        if (instructionsUI != null)
        {
            //instructionsUI.ShowInstruction("Reporte incorrecto. Verifica bien antes de reportar.");
        }
    }

    // NUEVO: Actualizar el contador de progreso si existe
    private void UpdateProgressCounter()
    {
        if (progressCounter != null && totalDamageParts > 0)
        {
            progressCounter.UpdateProgress(reportedDamages.Count, totalDamageParts);
        }
    }

    public void Dispatch()
    {
        AudioManager.Instance.Play("UI simple button");
        EvaluateResult();
    }

    public void SetTotalDamage(int value)
    {
        totalDamageParts = value;

        // NUEVO: Actualizar el contador cuando se establece el total
        UpdateProgressCounter();
    }

    private void EvaluateResult()
    {
        int missedReports = totalDamageParts - reportedDamages.Count;
        Debug.Log("Puntaje final: " + score);

        // NUEVO: Ocultar elementos de UI antes de mostrar resultados
        if (instructionsUI != null)
            instructionsUI.gameObject.SetActive(false);

        if (score >= 70)
            ApprovedModule(missedReports);
        else
            FailedModule(missedReports);

        SendResult();
    }

    private void ApprovedModule(int missedReports)
    {
        moduleApproved = true;
        AudioManager.Instance.Play("Great choose check");
        if (missedReports > 0)
        {
            resultMessage = "Has aprobado, sin embargo, has pasado por alto " + missedReports + " observaciónes. Recuerda que tu seguridad es primero, procura que siempre tus probabilidades de sufrir un accidente sean cero.";
        }
        else
        {
            resultMessage = "Buen trabajo, has realizado una inspección eficaz.";
        }
    }

    private void FailedModule(int missedReports)
    {
        AudioManager.Instance.Play("Wrong choose check");
        moduleApproved = false;
        resultMessage = "El vehículo es demasiado peligroso para estar operativo. No has realizado la inspección correctamente.";
    }

    private void SendResult()
    {
        ModuleResult result = new ModuleResult
        {
            moduleName = "Inspección Externa",
            approved = moduleApproved,
            score = score,
            missedDamages = totalDamageParts - reportedDamages.Count,
            message = resultMessage
        };

        GameManager.Instance.RegisterModuleResult(result);
    }
}
