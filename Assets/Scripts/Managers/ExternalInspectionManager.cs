using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExternalInspectionManager : MonoBehaviour
{
    public static Action<int> OnScoreAdded;

    [SerializeField] private Refillbar bar;

   

    public int score = 0;

    private List<DamagedZone> realDamages = new List<DamagedZone>();
    private List<DamagedZone> reportedDamages = new List<DamagedZone>();

    //public int wrongReports = 0;
    private int totalDamageParts;

    private string resultMessage;
    private bool moduleApproved;

    public void Start()
    {
        bar.Initialize(100); // puntaje máximo
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
            Debug.Log("Daño informado correctamente");

            OnScoreAdded?.Invoke(10);
        }
    }



    public void RegisterWrongReport()
    {
        //wrongReports++;
        //score -= 15;
        Debug.Log("Reporte incorrecto");
    }

    public void Dispatch()
    {
        // Daños no detectados
        //int missedDamages = realDamages.Count - reportedDamages.Count;

        //if (missedDamages > 0)
        //{
        //    score -= missedDamages * 25;
        //    Debug.Log("No se detectaron todos los daños");
        //}

        EvaluateResult();
    }

    public void SetTotalDamage(int value)
    {
        totalDamageParts = value;
    }

    private void EvaluateResult()
    {
        int missedReports = totalDamageParts - reportedDamages.Count;



        Debug.Log("Puntaje final: " + score);

        if (score >= 70)
            ApprovedModule(missedReports);
        else
            FailedModule(missedReports);

        SendResult();
    }

    private void ApprovedModule(int missedReports)
    {

        moduleApproved = true;

        if (missedReports > 0)
        {
            resultMessage = "Haz aprobado, sin embargo, has pasado por alto " + missedReports + " observaciones. Recuerda que tu seguridad es primero, procura que siempre tus probabilidades de sufrir un accidente sean cero.";
        }
        else

            resultMessage = "Felicitaciones, haz realizado una inspeccion eficaz.";
    }

    private void FailedModule(int missedReports)
    {
        moduleApproved = false;
        resultMessage = "El vehiculo es demasiado peligroso para estar operativo. No has realizado la inspeccion correctamente.";
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
