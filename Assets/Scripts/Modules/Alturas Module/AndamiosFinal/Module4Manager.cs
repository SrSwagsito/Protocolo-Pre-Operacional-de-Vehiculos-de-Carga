using System.Collections;
using UnityEngine;

/// <summary>
/// Orquestador del Módulo 4.
/// Coordina las dos fases, los sistemas de vidas, la transición de cámara y la UI.
/// NUNCA contiene lógica de gameplay directa.
///
/// Flujo:
///   StartStacking()
///     → OnCompleted (torre completa)
///       → CameraZoomController.ZoomOut()
///         → StartRopePhase()
///           → OnSwingEnded (llegó al final)
///             → RegisterResult(true)
///   En cualquier punto:
///     LifeSystem vacío → OnFail → HandleFail() → RegisterResult(false)
/// </summary>
public class Module4Manager : MonoBehaviour
{
    [Header("Fase 1 — Torre de Andamios")]
    public StackTowerController towerController;

    [Header("Fase 2 — Rapel")]

    [SerializeField] private TutorialPanel tutorialPopUp;
    public RopePlayerController  ropePlayer;
    public ObstacleSpawner       obstacleSpawner;
    public RopeGoal              ropeGoal;
    public RopeInputController   ropeInputController;

    [Header("Sprite del jugador (Fase 2)")]
    [Tooltip("GameObject con el sprite del personaje en la cuerda. Inactivo en Fase 1.")]
    public GameObject ropePlayerSprite;

    [Header("Cámara")]
    public CameraTransitionController cameraZoom;

    [Header("UI")]
    public Module4UIController uiController;

    [Header("Vidas — configurable desde el Editor")]
    [Min(1)] public int stackMaxLives = 3;
    [Min(1)] public int ropeMaxLives  = 3;

    [Header("Configuración del módulo")]
    public string moduleName = "Module4_Andamio";

    // ─── Estado interno ───────────────────────────────────────────────────────
    private int         _score;
    private int         _mistakes;
    private ModuleState _state;
    private LifeSystemFinal  _stackLives;
    private LifeSystemFinal _ropeLives;

    private bool gameplayStarted;

    private enum ModuleState { Idle, Stacking, Rope, Completed, Failed }

    // ─── Inicialización ───────────────────────────────────────────────────────

    private void OnEnable()
    {
        if (tutorialPopUp != null)
            tutorialPopUp.OnTutorialClosed += StartGameplay;


    }

    private void OnDisable()
    {
        if (tutorialPopUp != null)
            tutorialPopUp.OnTutorialClosed -= StartGameplay;

    }

    private void Start()
    {
        Debug.Log("START MODULE");

        InitializeModule();

        if (tutorialPopUp != null)
            tutorialPopUp.ShowTutorial();
        else
            StartGameplay();
    }

    private void InitializeModule()
    {
        _stackLives = new LifeSystemFinal(stackMaxLives);
        _ropeLives = new LifeSystemFinal(ropeMaxLives);

        uiController?.InitializeStackLives(stackMaxLives);
        uiController?.InitializeRopeLives(ropeMaxLives);

        _stackLives.OnLivesChanged += (c, m) => uiController?.UpdateStackLives(c, m);
        _ropeLives.OnLivesChanged += (c, m) => uiController?.UpdateRopeLives(c, m);

        towerController.Initialize(_stackLives);
        ropePlayer.Initialize(_ropeLives, ropeInputController);
        ropeGoal.Initialize(ropePlayer);

        towerController.OnBlockPlaced += HandleBlockPlaced;
        towerController.OnCompleted += HandleStackCompleted;
        towerController.OnFail += HandleFail;

        ropePlayer.OnSwingEnded += HandleSwingEnded;
        ropePlayer.OnFail += HandleFail;

        ropePlayerSprite?.SetActive(false);
    }

    private void StartGameplay()
    {
        if (gameplayStarted)
            return;

        gameplayStarted = true;

        StartStacking();
    }

    // ─── Fase 1 ───────────────────────────────────────────────────────────────

    private void StartStacking()
    {
        _state = ModuleState.Stacking;
        uiController?.ShowStackPanel();
        towerController.StartPhase();
    }

    private void HandleBlockPlaced(bool perfect)
    {
        _score += perfect ? 10 : 5;
    }

    private void HandleStackCompleted()
    {
        cameraZoom.TransitionToRopeView();
        // El blend dura ~2s; si quieres esperar antes de activar la Fase 2:
        StartCoroutine(StartRopeAfterTransition());
    }

    private IEnumerator StartRopeAfterTransition()
    {
        yield return new WaitForSeconds(2f); // igual que la duración del blend
        StartRopePhase();

        uiController?.ShowRopePanel();
    }

    // ─── Transición y Fase 2 ─────────────────────────────────────────────────

    private void StartRopePhase()
    {
        _state = ModuleState.Rope;

        // Mostrar sprite del personaje en la cuerda
        ropePlayerSprite?.SetActive(true);

        // Activar obstáculos y movimiento
        obstacleSpawner.StartSpawning();
        ropePlayer.EnableMovement();
    }

    private void HandleSwingEnded()
    {
        obstacleSpawner.StopSpawning();
        _state = ModuleState.Completed;
        RegisterResult(true, "Módulo completado exitosamente.");
    }

    // ─── Fallo ────────────────────────────────────────────────────────────────

    private void HandleFail()
    {
        if (_state == ModuleState.Failed) return; // guardia contra doble disparo

        obstacleSpawner.StopSpawning();
        ropePlayer.DisableMovement();
        towerController.StopPhase();
        _mistakes++;
        _state = ModuleState.Failed;
        RegisterResult(false, "El módulo ha fallado.");
    }

    // ─── Resultado final ──────────────────────────────────────────────────────

    private void RegisterResult(bool approved, string message)
    {
        var result = new ModuleResult
        {
            moduleName    = moduleName,
            approved      = approved,
            score         = _score,
            missedDamages = _mistakes,
            message       = message
        };
        GameManager.Instance.RegisterModuleResult(result);
    }
}
