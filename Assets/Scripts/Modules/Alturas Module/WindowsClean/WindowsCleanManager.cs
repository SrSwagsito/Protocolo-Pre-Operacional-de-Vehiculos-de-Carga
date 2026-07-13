using System;
using UnityEngine;

public class WindowsCleanManager : MonoBehaviour
{

    [Header("Tutorial variables")]
    [SerializeField] private ProgressCounter progressCounter;
    [SerializeField] private TutorialPanel tutorial;
    [SerializeField] private InstructionsUI instructionsUI;

    [Header("Level variables")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlatformMovement movement;
    [SerializeField] private LifeSystem lifeSystem;
    [SerializeField] private CleaningTargetController windowsTarget;
    [SerializeField] private PedestrianController pedestrianController;


    private bool moduleFinished;

    public event Action<int, int> OnProgressChanged;
    public event Action OnAllTargetsCleaned;
    public event Action OnLevelStarted;

    private void Awake()
    {
    }


    private void OnEnable()
    {
        lifeSystem.OnGameOver += HandleGameOver;
        windowsTarget.OnAllTargetsCleaned += HandleVictory;
        windowsTarget.OnProgressChanged += UpdateProgress;
        pedestrianController.PedestrianToLeaving += HandleGameOver;

        if (tutorial != null)
            tutorial.OnTutorialClosed += StartCleaning;
    }

    private void OnDisable()
    {
        lifeSystem.OnGameOver -= HandleGameOver;
        windowsTarget.OnAllTargetsCleaned -= HandleVictory;
        windowsTarget.OnProgressChanged -= UpdateProgress;
        pedestrianController.PedestrianToLeaving -= HandleGameOver;

        if (tutorial != null)
            tutorial.OnTutorialClosed -= StartCleaning;
    }

    private void Start()
    {
        if (tutorial != null)
            tutorial.ShowTutorial();

        else
            StartCleaning();
    }

    private void StartCleaning()
    {

        instructionsUI.ShowInstruction("Limpia todas las ventanas para terminar tu turno");

        progressCounter.UpdateProgress(windowsTarget.CleanedCount, windowsTarget.TotalTargets);

        OnLevelStarted?.Invoke();
    }

    private void HandlePedestrianAccident()
    {
        if (moduleFinished)
            return;

        moduleFinished = true;

        SendResult(false);
    }

    private void HandleGameOver()
    {
        animator.SetTrigger("Lose");
        if (moduleFinished)
            return;

        moduleFinished = true;

        SendResult(false);

    }

    private void HandleVictory()
    {
        if (moduleFinished)
            return;

        moduleFinished = true;

        SendResult(true);
    }

    public void UpdateProgress(int cleaned, int total)
    {
        progressCounter.UpdateProgress(windowsTarget.CleanedCount, windowsTarget.TotalTargets);
    }

    private int CalculateScore()
    {
        return windowsTarget.CleanedCount * 10;
    }

    private int CalculateMistakes()
    {
        return lifeSystem.maxLifes - lifeSystem.currentLifes;
    }

    private void SendResult(bool success)
    {
        ModuleResult result =
            new ModuleResult
            {
                moduleName = "Limpieza de Ventanas",
                approved = success,
                score = CalculateScore(),
                missedDamages = CalculateMistakes(),
                message = GenerateDetailMessage()
            };

        GameManager.Instance.RegisterModuleResult(result);
    }

    private string GenerateDetailMessage()
    {
        string message = $"Ventanas limpiadas: {windowsTarget.CleanedCount}/{windowsTarget.TotalTargets}\n";

        //if (windowsTarget.CleanedCount >= requiredCloths && incorrectClothsSubmitted == 0)
        //{
        //    message += "¡Perfecto! Identificaste todos los documentos necesarios sin errores.";
        //}
        //else if (correctClothsSubmitted >= requiredCloths)
        //{
        //    message += "Identificaste todos los documentos, pero incluiste algunos innecesarios.";
        //}
        //else
        //{
        //    int missing = requiredCloths - correctClothsSubmitted;
        //    message += $"Te faltaron {missing} documento(s) necesario(s). Revisa la lista de documentación requerida.";
        //}

        return message;
    }
}
