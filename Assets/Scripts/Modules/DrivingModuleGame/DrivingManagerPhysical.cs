using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class DrivingModuleManagerPhysical : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TruckPhysicalController truck;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private RoadPivotSystem roadSystem;
    [SerializeField] private ProgressCounter progressCounter;
    [SerializeField] private TutorialPanel tutorial;
    [SerializeField] private InstructionsUI instructionsUI;

    [Header("UI Panels")]
    [SerializeField] private GameObject difficultySelectionPanel;
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject gameplayUI;

    [Header("Progress UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider distanceSlider;
    [SerializeField] private TextMeshProUGUI distanceText;

    [Header("Parallax")]
    [SerializeField] private ParallaxMovement[] parallaxLayers;

    [Header("Configuration")]
    [SerializeField] private float easyDuration;
    [SerializeField] private float hardDuration;
    [SerializeField] private float countdownTime;

    [Header("Button COnfiguration")]
    [SerializeField] GameObject buttonsMobilePanel;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    [Header("Ending Cinematics")]
    [SerializeField] private float slowdownDuration = 2f;
    [SerializeField] private float arrivalDelay = 1f;
    [SerializeField] private GameObject destinationMarker; // Opcional: sprite de meta

    // Estado
    private int score = 0;
    private int obstacleHits = 0;
    private int boundaryHits = 0;
    private float timer;
    private float totalDuration;
    private bool moduleFinished = false;
    private bool isPlaying = false;
    private bool isChallengeMode = false;

    private void OnEnable()
    {
        truck.OnCollectibleHit += AddPoints;
        truck.OnObstacleHit += SubtractPoints;
        truck.OnBoundaryPenalty += OnBoundaryHit;

        if (tutorial != null)
        {
            tutorial.OnTutorialClosed += ShowDifficultySelection;
        }

        SetUpMobileSettings();
    }

    private void OnDisable()
    {
        truck.OnCollectibleHit -= AddPoints;
        truck.OnObstacleHit -= SubtractPoints;
        truck.OnBoundaryPenalty -= OnBoundaryHit;

        if (tutorial != null)
        {
            tutorial.OnTutorialClosed -= ShowDifficultySelection;
        }


        if (upButton)
        {
            upButton.onClick.RemoveAllListeners();
        }
        if (downButton)
        {
            downButton.onClick.RemoveAllListeners();
        }
    }

    private void Start()
    {
        PauseAllSystems();

        // Ocultar UI
        HideAllUI();

        ShowMobileControls();

        // Mostrar tutorial
        if (tutorial != null)
        {
            tutorial.ShowTutorial();
        }
        else
        {
            ShowDifficultySelection();
        }
    }

    private void Update()
    {
        if (!isPlaying || moduleFinished) return;

        // Actualizar timer
    AudioManager.Instance.Play("Truck running");
        timer -= Time.deltaTime;
        UpdateProgressUI();

        // Verificar fin
        if (timer <= 0f)
        {
            timer = 0f;
            StartCoroutine(FinishModuleCinematic());
        }
    }

    private void SetUpMobileSettings()
    {
        if (upButton != null)
        {
            upButton.onClick.AddListener(() => { if (truck != null && isPlaying && !moduleFinished) { truck.MoveUp(); } });
        }
        if (downButton != null)
        {
            downButton.onClick.AddListener(() => { if (truck != null && isPlaying && !moduleFinished) { truck.MoveDown(); } });
        }
    }

    private void ShowMobileControls()
    {
        if (buttonsMobilePanel == null) return;

        #if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
        buttonsMobilePanel.SetActive(true);
        #else
        buttonsMobilePanel.SetActive(false);
        #endif
    }

    private void PauseAllSystems()
    {
        // Pausar carretera
        if (roadSystem != null)
        {
            roadSystem.SetActive(false);
        }

        // Pausar parallax
        SetParallaxActive(false);

        // Asegurarse de que no haya spawning
        if (spawnManager != null)
        {
            spawnManager.StopSpawning();
        }

        if (truck != null)
        {
            truck.SetInputEnabled(false); 
        }

    }

    private void ResumeAllSystems()
    {
        // Activar carretera
        if (roadSystem != null)
        {
            roadSystem.SetActive(true);
        }

        // Activar parallax
        SetParallaxActive(true);

        // Iniciar spawning
        if (spawnManager != null)
        {
            spawnManager.StartSpawning();
        }

        if (truck != null)
        {
            truck.SetInputEnabled(true);
        }
    }



    /// <summary>
    /// Oculta toda la UI al inicio
    /// </summary>
    private void HideAllUI()
    {
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.SetActive(false);

        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(false);
        if (destinationMarker != null)
            destinationMarker.SetActive(false);
    }

    /// <summary>
    /// Muestra panel de selección de dificultad
    /// </summary>
    private void ShowDifficultySelection()
    {
        if (difficultySelectionPanel != null)
        {
            difficultySelectionPanel.SetActive(true);
        }
        else
        {
            // Si no hay panel, comenzar con dificultad normal
            OnDifficultySelected(false);
        }
    }

    /// <summary>
    /// Llamado por los botones de dificultad
    /// </summary>
    public void OnEasySelected()
    {
        OnDifficultySelected(false);
    }

    public void OnHardSelected()
    {
        OnDifficultySelected(true);
    }

    private void OnDifficultySelected(bool isHard)
    {
        isChallengeMode = isHard;
        totalDuration = isHard ? hardDuration : easyDuration;
        timer = totalDuration;

        // Ocultar panel de selección
        if (difficultySelectionPanel != null)
            difficultySelectionPanel.SetActive(false);

        // Configurar dificultad ANTES de iniciar
        if (spawnManager != null)
            spawnManager.SetDifficulty(isChallengeMode);

        if (roadSystem != null)
            roadSystem.SetDifficulty(isChallengeMode);

        // Iniciar cuenta regresiva
        StartCoroutine(CountdownSequence());
    }

    /// <summary>
    /// Cuenta regresiva antes de empezar
    /// </summary>
    private IEnumerator CountdownSequence()
    {
        if (countdownPanel != null)
            countdownPanel.SetActive(true);

        // Cuenta regresiva: 3, 2, 1, ¡GO!
        for (int i = (int)countdownTime; i > 0; i--)
        {
            if (countdownText != null)

                AudioManager.Instance.Play("Count normal");
                countdownText.text = i.ToString();

            yield return new WaitForSeconds(1f);
        }

        // ¡GO!
        if (countdownText != null)
            AudioManager.Instance.Play("Count final");
        countdownText.text = "¡Vamos!";

        yield return new WaitForSeconds(0.5f);

        // Ocultar cuenta regresiva
        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        // Iniciar juego
        StartDriving();
    }

    /// <summary>
    /// Inicia el gameplay
    /// </summary>
    private void StartDriving()
    {
        // Mostrar UI de gameplay
        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        // ACTIVAR TODOS LOS SISTEMAS
        ResumeAllSystems();
        AudioManager.Instance.Play("Truck beginning");

        // Marcar como jugando
        isPlaying = true;

        // Instrucción
        if (instructionsUI != null)
        {
            string instruction = isChallengeMode
                ? "¡Ruta difícil! Mantén el control."
                : "¡Mantente en el carril y recoge objetos!";
            instructionsUI.ShowInstruction(instruction);
        }

        UpdateProgressUI();
    }

    /// <summary>
    /// Actualiza la UI de progreso (tiempo y distancia)
    /// </summary>
    private void UpdateProgressUI()
    {
        // Timer
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);

            if (timer <= 0)
            {
                minutes = 0;
                seconds = 0;

            }
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // Barra de distancia (inversa: empieza llena, termina vacía)
        float progress = timer / totalDuration;

        if (distanceSlider != null)
        {
            distanceSlider.value = 1f - progress; // Inverso para que se llene
        }

        // Texto de distancia (porcentaje completado)
        if (distanceText != null)
        {
            int percentage = Mathf.FloorToInt((1f - progress) * 100f);
            distanceText.text = $"{percentage}%";
        }
    }

    /// <summary>
    /// Controla parallax (activar/desactivar)
    /// </summary>
    private void SetParallaxActive(bool active)
    {
        if (parallaxLayers == null) return;

        foreach (var parallax in parallaxLayers)
        {
            if (parallax != null)
                parallax.enabled = active;
        }
    }

    /// <summary>
    /// Finalización cinematográfica con slowdown
    /// </summary>
    private IEnumerator FinishModuleCinematic()
    {
        if (moduleFinished) yield break;
        moduleFinished = true;

        // Detener spawning
        spawnManager.StopSpawning();

        // Mensaje de llegada
        if (instructionsUI != null)
            instructionsUI.ShowInstruction("¡Llegando al destino!");

        // Slowdown gradual
        AudioManager.Instance.Play("Truck stopping");
        float elapsed = 0f;
        float originalTimeScale = Time.timeScale;

        while (elapsed < slowdownDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / slowdownDuration;

            // Reducir velocidad del juego gradualmente
            Time.timeScale = Mathf.Lerp(originalTimeScale, 0.2f, progress);

            yield return null;
        }

        // Mostrar destino (opcional)
        if (destinationMarker != null)
        {
            destinationMarker.SetActive(true);
        }

        // Esperar un momento en slow motion
        yield return new WaitForSecondsRealtime(arrivalDelay);

        // Restaurar velocidad
        Time.timeScale = originalTimeScale;

        // Ocultar gameplay UI
        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        // Pausar parallax
        SetParallaxActive(false);

        PauseAllSystems();

        // Mostrar resultados
        SendResult();
    }

    private void AddPoints(int points)
    {
        score += points;
        progressCounter.UpdateProgress(score, 1000);
    }

    private void SubtractPoints(int points)
    {
        obstacleHits++;
        score -= points;
        if (score < 0) score = 0;
        progressCounter.UpdateProgress(score, 1000);
    }

    private void OnBoundaryHit(int penalty)
    {
        boundaryHits++;
        score -= penalty;
        if (score < 0) score = 0;
        progressCounter.UpdateProgress(score, 1000);
    }

    public void SendResult()
    {
        string difficulty = isChallengeMode ? "Difícil" : "Fácil";

        ModuleResult result = new ModuleResult
        {
            moduleName = $"Recorrido ({difficulty})",
            approved = score >= 0,
            score = score,
            missedDamages = obstacleHits + boundaryHits,
            message = GenerateFinalMessage()
        };

        GameManager.Instance.RegisterModuleResult(result);
    }

    private string GenerateFinalMessage()
    {
        string difficulty = isChallengeMode ? "difícil" : "fácil";

        string message = $"Ruta {difficulty} completada\n\n";
        message += $"Puntuación final: {score}\n";
        //message += $"Obstáculos golpeados: {obstacleHits}\n";
        //message += $"Toques en bordes: {boundaryHits}\n\n";

        if (score >= 0)
        {
            if (isChallengeMode)
                message += "¡Impresionante! Gracias por probar la dificultad dificil.";
            else
                message += "¡Excelente trabajo! Has completado el recorrido con éxito.";
        }

        return message;
    }
}