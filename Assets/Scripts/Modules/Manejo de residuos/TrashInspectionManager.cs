using System;
using System.Collections.Generic;
using UnityEngine;

public class TrashInspectionManager : MonoBehaviour
{
    public static TrashInspectionManager Instance;

    // Eventos para que la UI (barra de vidas, puntos, etc.) reaccione
    public static Action<int> OnScoreAdded;      // manda el puntaje actual
    public static Action<int> OnLivesChanged;    // manda las vidas actuales
    public static Action OnGameOver;             // se dispara al perder todas las vidas

    [Header("UI References")]
    [SerializeField] private TutorialPanel tutorialPopup;
    [SerializeField] private InstructionsUI instructionsUI;

    [Header("Settings")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int pointsPerCorrect = 10;
    [SerializeField] private int approvalThreshold = 70;

    public int score { get; private set; } = 0;
    public int lives { get; private set; }

    private List<TrashType> correctTrash = new List<TrashType>();
    private List<TrashType> wrongTrash = new List<TrashType>();
    private int totalDamageParts;
    private string resultMessage;
    private bool moduleApproved;
    private bool tutorialShown = false;
    private bool inspectionEnded = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (tutorialPopup != null)
        {
            tutorialPopup.OnTutorialClosed += OnTutorialClosed;
        }
    }

    private void OnDisable()
    {
        if (tutorialPopup != null)
        {
            tutorialPopup.OnTutorialClosed -= OnTutorialClosed;
        }
    }
    public static Action<bool, int, string> OnInspectionEnded; // NUEVO

    private void Start()
    {
        lives = maxLives;
        score = 0;

        OnScoreAdded?.Invoke(score);
        OnLivesChanged?.Invoke(lives);

        if (tutorialPopup != null && !tutorialShown)
        {
            tutorialPopup.ShowTutorial();
            tutorialShown = true;
        }
        else
        {
            StartInspection();
        }
    }

    private void OnTutorialClosed()
    {
        StartInspection();
    }

    private void StartInspection()
    {
        if (instructionsUI != null)
        {
            instructionsUI.ShowInstruction("Inspecciona el vehículo y reporta los daños que encuentres.");
        }

        Debug.Log("Inspección iniciada");
    }

    public void SetTotalDamage(int value)
    {
        totalDamageParts = value;
    }

    // Llamado cuando la basura cae en la caneca correcta
    public void RegisterCorrectTrash(TrashType trash)
    {
        if (inspectionEnded) return;

        if (!correctTrash.Contains(trash))
        {
            correctTrash.Add(trash);

            score += pointsPerCorrect;

            AudioManager.Instance.Play("UI simple button");

            OnScoreAdded?.Invoke(score);

            if (instructionsUI != null)
            {
                instructionsUI.ShowInstruction(
                    $"Basuras correctas: {correctTrash.Count}/{totalDamageParts}"
                );
            }

            Debug.Log("Basura correcta. Puntaje: " + score);
        }
    }

    // Llamado cuando la basura cae en la caneca incorrecta
    public void RegisterWrongTrash(TrashType trash)
    {
        if (inspectionEnded) return;

        if (!wrongTrash.Contains(trash))
        {
            wrongTrash.Add(trash);

            AudioManager.Instance.Play("Wrong choose check");

            LoseLife();

            Debug.Log("Basura incorrecta. Vidas: " + lives);
        }
    }

    // Llamado cuando el jugador reporta mal un daño
    public void RegisterWrongReport()
    {
        if (inspectionEnded) return;

        AudioManager.Instance.Play("Wrong choose check");

        LoseLife();

        Debug.Log("Reporte incorrecto. Vidas: " + lives);
    }

    private void LoseLife()
    {
        lives = Mathf.Max(0, lives - 1);

        OnLivesChanged?.Invoke(lives);

        if (lives <= 0)
        {
            EndInspection(forcedFail: true);
        }
    }

    public void FinishInspection()
    {
        EndInspection(forcedFail: false);
    }

    private void EndInspection(bool forcedFail)
    {
       
        if (inspectionEnded) return;
        inspectionEnded = true;

        int missedReports = totalDamageParts - wrongTrash.Count;

        if (instructionsUI != null)
            instructionsUI.gameObject.SetActive(false);

        if (forcedFail)
        {
            moduleApproved = false;
            AudioManager.Instance.Play("Wrong choose check");
            resultMessage = "El vehículo es demasiado peligroso para estar operativo. Te quedaste sin vidas.";
            OnGameOver?.Invoke();
        }
        else if (score >= approvalThreshold)
        {
            moduleApproved = true;
            AudioManager.Instance.Play("Great choose check");
            resultMessage = missedReports > 0
                ? "Has aprobado, sin embargo, has pasado por alto " + missedReports + " observaciones. Recuerda que tu seguridad es primero."
                : "Buen trabajo, has realizado una inspección eficaz.";
        }
        else
        {
            moduleApproved = false;
            AudioManager.Instance.Play("Wrong choose check");
            resultMessage = "El vehículo es demasiado peligroso para estar operativo. No has realizado la inspección correctamente.";
        }

        Debug.Log("Puntaje final: " + score);
        OnInspectionEnded?.Invoke(moduleApproved, score, resultMessage);
        SendResult();
    }

    private void SendResult()
    {
        ModuleResult result = new ModuleResult
        {
            moduleName = "Inspección Externa",
            approved = moduleApproved,
            score = score,
            missedDamages = totalDamageParts - wrongTrash.Count,
            message = resultMessage
        };

        GameManager.Instance.RegisterModuleResult(result);
    }
}