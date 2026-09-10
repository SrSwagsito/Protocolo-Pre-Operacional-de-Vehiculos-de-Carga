
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ManagerRecolection : MonoBehaviour
{
    public static ManagerRecolection Instance;

    // =========================================================
    // EVENTOS
    // =========================================================

    public static Action<int> OnLivesChanged;
    public static Action<int> OnScoreAdded;
    public static Action OnGameOver;

    // Resultado final de la inspección/recolección
    public static Action<bool, int, string> OnInspectionEnded;


    // =========================================================
    // UI REFERENCES
    // =========================================================

    [Header("UI References")]
    [SerializeField] private TutorialPanel tutorialPopup;
    [SerializeField] private InstructionsUI instructionsUI;
    [SerializeField] private ProgressCounterTrash progressCounter;

    public TextMeshProUGUI scoreText;


    // =========================================================
    // CONFIGURACIÓN DE VIDAS Y PUNTAJE
    // =========================================================

    [Header("Configuración")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int pointsPerCorrect = 10;
    [SerializeField] private int approvalThreshold = 70;
    [SerializeField] private CameraFollow cameraFollow;

    // =========================================================
    // SISTEMA DE RECOLECCIÓN
    // =========================================================

    [Header("Jugadores")]
    public Transform player;
    public Transform playerTwo;

    private Transform[] players;

    [Header("Triggers de niveles")]
    public BoxCollider2D[] triggers;


    // =========================================================
    // ESTADO DEL JUEGO
    // =========================================================

    public int Score { get; private set; }
    public int Health { get; private set; }

    // Mantengo health como propiedad por compatibilidad
    // con otros scripts que posiblemente la estén utilizando.
    public int health => Health;


    // =========================================================
    // SISTEMA DE INSPECCIÓN / CLASIFICACIÓN
    // =========================================================

    [Header("Configuración de inspección")]

    [SerializeField]
    private int totalDamageParts = 20;

    [SerializeField]
    private bool ismodule2 = false;

    private List<TrashType> correctTrash = new List<TrashType>();
    private List<TrashType> wrongTrash = new List<TrashType>();

    private string resultMessage;
    private bool moduleApproved;

    private bool tutorialShown = false;
    private bool inspectionEnded = false;
    private bool level2Unlocked;
    private bool level3Unlocked;
    private bool moduleCompleted;

    private void CheckProgress()
    {
        if (!level2Unlocked && Score >= 10)
        {
            level2Unlocked = true;

            Debug.Log("Nivel 2 desbloqueado");

            NextLevel();
        }

        if (!level3Unlocked && Score >= 20)
        {
            level3Unlocked = true;

            Debug.Log("Nivel 3 desbloqueado");

            Level3();
        }

        if (!moduleCompleted && Score >= 40)
        {
            moduleCompleted = true;

            Debug.Log("Módulo completado");

            NextModule();
        }
    }

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        Instance = this;
    }


    // =========================================================
    // ENABLE / DISABLE
    // =========================================================

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


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Inicializar vidas y puntaje
        Health = maxLives;
        Score = 0;

        // Jugadores
        players = new Transform[2];

        players[0] = playerTwo;
        players[1] = player;

        // Actualizar UI
        OnLivesChanged?.Invoke(Health);
        OnScoreAdded?.Invoke(Score);

        if (scoreText != null)
        {
            scoreText.text = "Puntos: " + Score;
        }

        // Mostrar tutorial
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


    // =========================================================
    // TUTORIAL
    // =========================================================

    private void OnTutorialClosed()
    {
        StartInspection();
    }


    // =========================================================
    // INICIAR INSPECCIÓN
    // =========================================================

    private void StartInspection()
    {
        if (instructionsUI != null)
        {
            instructionsUI.ShowInstruction(
                "Inspecciona el vehículo y reporta los daños que encuentres."
            );
        }

        Debug.Log("Inspección/recolección iniciada.");
    }


    // =========================================================
    // CONFIGURAR TOTAL DE RESIDUOS
    // =========================================================

    public void SetTotalDamage(int value)
    {
        totalDamageParts = value;

        Debug.Log("Total residuos = " + totalDamageParts);

        UpdateProgressUI();
    }


    // =========================================================
    // AGREGAR PUNTAJE
    // =========================================================

    public void AddScore(int points)
    {
        if (inspectionEnded)
            return;

        Score += points;

        if (scoreText != null)
        {
            scoreText.text = "Puntos: " + Score;
        }

        OnScoreAdded?.Invoke(Score);

        Debug.Log("Puntaje actual: " + Score);

        CheckProgress();
    }


    // =========================================================
    // BASURA CORRECTA
    // =========================================================

    public void RegisterCorrectTrash(TrashType trash)
    {
        if (inspectionEnded)
            return;

        // Evita registrar dos veces la misma basura
        if (!correctTrash.Contains(trash))
        {
            correctTrash.Add(trash);

            // Puntos
            Score += pointsPerCorrect;

            if (scoreText != null)
            {
                scoreText.text = "Puntos: " + Score;
            }

            // Actualizar progreso
            UpdateProgressUI();

            // Sonido
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("UI simple button");
            }

            // Notificar puntaje
            OnScoreAdded?.Invoke(Score);

            // Mostrar instrucciones
            if (instructionsUI != null)
            {
                instructionsUI.ShowInstruction(
                    $"Basuras correctas: {correctTrash.Count}/{totalDamageParts}"
                );
            }

            Debug.Log(
                "Basura correcta. Puntaje: " + Score
            );

            // Revisar progreso
            CheckProgress();
        }
    }


    // =========================================================
    // BASURA INCORRECTA
    // =========================================================

    public void RegisterWrongTrash(TrashType trash)
    {
        if (inspectionEnded)
            return;

        // Evita registrar dos veces la misma basura
        if (!wrongTrash.Contains(trash))
        {
            wrongTrash.Add(trash);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("Wrong choose check");
            }

            LoseLife();

            Debug.Log(
                "Basura incorrecta. Vidas: " + Health
            );
        }
    }


    // =========================================================
    // REPORTE INCORRECTO
    // =========================================================

    public void RegisterWrongReport()
    {
        if (inspectionEnded)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("Wrong choose check");
        }

        LoseLife();

        Debug.Log(
            "Reporte incorrecto. Vidas: " + Health
        );
    }


    // =========================================================
    // PERDER VIDA
    // =========================================================

    public void LoseLife()
    {
        Health = Mathf.Max(0, Health - 1);

        // Notificar a la UI
        OnLivesChanged?.Invoke(Health);

        Debug.Log(
            "Vida perdida. Vidas restantes: " + Health
        );

        // Game Over
        if (Health <= 0)
        {
            EndInspection(true);
        }
    }


    // =========================================================
    // FINALIZAR INSPECCIÓN
    // =========================================================

    public void FinishInspection()
    {
        EndInspection(false);
    }


    // =========================================================
    // FINALIZAR INSPECCIÓN
    // =========================================================

    private void EndInspection(bool forcedFail)
    {
        if (inspectionEnded)
            return;

        inspectionEnded = true;

        int missedReports =
            totalDamageParts - wrongTrash.Count;


        // Ocultar instrucciones
        if (instructionsUI != null)
        {
            instructionsUI.gameObject.SetActive(false);
        }


        // =====================================================
        // PERDIÓ TODAS LAS VIDAS
        // =====================================================

        if (forcedFail)
        {
            moduleApproved = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("Wrong choose check");
            }

            resultMessage =
                "El vehículo es demasiado peligroso para estar operativo. Te quedaste sin vidas.";

            OnGameOver?.Invoke();
        }


        // =====================================================
        // APROBÓ
        // =====================================================

        else if (Score >= approvalThreshold)
        {
            moduleApproved = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("Great choose check");
            }

            resultMessage =
                missedReports > 0
                ? "Has aprobado, sin embargo, has pasado por alto "
                  + missedReports
                  + " observaciones. Recuerda que tu seguridad es primero."
                : "Buen trabajo, has realizado una inspección eficaz.";
        }


        // =====================================================
        // NO APROBÓ
        // =====================================================

        else
        {
            moduleApproved = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("Wrong choose check");
            }

            resultMessage =
                "El vehículo es demasiado peligroso para estar operativo. No has realizado la inspección correctamente.";
        }


        Debug.Log(
            "Puntaje final: " + Score
        );

        Debug.Log(
            "Inspección finalizada. Aprobado: "
            + moduleApproved
        );


        // Notificar resultado
        OnInspectionEnded?.Invoke(
            moduleApproved,
            Score,
            resultMessage
        );


        // Enviar resultado al GameManager
        SendResult();
    }


    // =========================================================
    // ENVIAR RESULTADO AL GAME MANAGER
    // =========================================================

    private void SendResult()
    {
        ModuleResult result = new ModuleResult
        {
            moduleName = ismodule2
                ? "Clasificación de residuos aeropuerto"
                : "Clasificación de residuos",

            approved = moduleApproved,

            score = Score,

            missedDamages =
                totalDamageParts - wrongTrash.Count,

            message = resultMessage
        };


        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterModuleResult(result);
        }
        else
        {
            Debug.LogWarning(
                "GameManager.Instance no existe. No se pudo registrar el resultado."
            );
        }
    }


    // =========================================================
    // ACTUALIZAR PROGRESO
    // =========================================================

    private void UpdateProgressUI()
    {
        Debug.Log(
            $"Progreso: {correctTrash.Count}/{totalDamageParts}"
        );

        if (progressCounter != null)
        {
            progressCounter.UpdateProgress(
                correctTrash.Count,
                totalDamageParts
            );
        }
    }


    // =========================================================
    // PROGRESIÓN DE NIVELES
    // =========================================================

 


    // =========================================================
    // NIVEL 2
    // =========================================================

    public void NextLevel()
    {
        if (player != null)
        {
            player.position = new Vector3(
                player.position.x,
                2.855f,
                player.position.z

            );
            cameraFollow.SetLevelY(5.84f);
        }

        if (playerTwo != null)
        {
            playerTwo.position = new Vector3(
                playerTwo.position.x,
                5.2f,
                playerTwo.position.z
            );
        }

        if (triggers != null && triggers.Length > 1)
        {
            triggers[1].isTrigger = false;
        }

        Debug.Log("Nivel 2 desbloqueado.");
    }


    // =========================================================
    // NIVEL 3
    // =========================================================

    public void Level3()
    {
        if (player != null)
        {
            player.position = new Vector3(
                player.position.x,
                15.377f,
                player.position.z
            );
            cameraFollow.SetLevelY(18.6f);
        }

        if (playerTwo != null)
        {
            playerTwo.position = new Vector3(
                playerTwo.position.x,
                12.58f,
                playerTwo.position.z
            );
        }

        if (triggers != null && triggers.Length > 2)
        {
            triggers[2].isTrigger = false;
        }

        Debug.Log("Nivel 3 desbloqueado.");
    }


    // =========================================================
    // SIGUIENTE MÓDULO
    // =========================================================

    public void NextModule()
    {
        SceneManager.LoadScene("parttrhee");
    }
}

