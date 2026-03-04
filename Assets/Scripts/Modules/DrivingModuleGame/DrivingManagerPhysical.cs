using UnityEngine;

/// <summary>
/// Manager actualizado para el sistema de pivotes físicos
/// </summary>
public class DrivingModuleManagerPhysical : MonoBehaviour
{
    [SerializeField] private TruckPhysicalController truck;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private RoadPivotSystem roadSystem;
    [SerializeField] private ProgressCounter progressCounter;
    [SerializeField] private TutorialPanel tutorial;
    [SerializeField] private InstructionsUI instructionsUI;

    [Header("Configuration")]
    [SerializeField] private float levelDuration = 60f;
    [SerializeField] private bool isChallengeMode = false;
    
    private int score = 0;
    private int obstacleHits = 0;
    private int boundaryHits = 0;
    private float timer;
    private bool moduleFinished = false;

    private void OnEnable()
    {
        truck.OnCollectibleHit += AddPoints;
        truck.OnObstacleHit += SubtractPoints;
        truck.OnBoundaryPenalty += OnBoundaryHit;
        
        if (tutorial != null)
        {
            tutorial.OnTutorialClosed += StartDriving;
        }
    }

    private void OnDisable()
    {
        truck.OnCollectibleHit -= AddPoints;
        truck.OnObstacleHit -= SubtractPoints;
        truck.OnBoundaryPenalty -= OnBoundaryHit;
        
        if (tutorial != null)
        {
            tutorial.OnTutorialClosed -= StartDriving;
        }
    }

    private void Start()
    {
        if (tutorial != null)
        {
            tutorial.ShowTutorial();
        }
        else
        {
            StartDriving();
        }
    }

    private void StartDriving()
    {
        instructionsUI.ShowInstruction("¡Evita tocar los bordes del carril y recoge objetos!");
        
        spawnManager.SetDifficulty(isChallengeMode);
        roadSystem.SetDifficulty(isChallengeMode);
        
        spawnManager.StartSpawning();
        timer = levelDuration;
    }

    private void Update()
    {
        if (moduleFinished) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            FinishModule();
        }
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
        
        Debug.Log($"Toca borde! Penalización: -{penalty}. Score: {score}");
    }

    public void FinishModule()
    {
        moduleFinished = true;
        spawnManager.StopSpawning();
        SendResult();
    }

    public void SendResult()
    {
        ModuleResult result = new ModuleResult
        {
            moduleName = "Recorrido",
            approved = score >= 1000,
            score = score,
            missedDamages = obstacleHits + boundaryHits,
            message = GenerateFinalMessage()
        };

        GameManager.Instance.RegisterModuleResult(result);
    }

    private string GenerateFinalMessage()
    {
        string message = $"Puntuación final: {score}\n";
        message += $"Obstáculos golpeados: {obstacleHits}\n";
        message += $"Toques en bordes: {boundaryHits}\n\n";
        
        if (score >= 1000)
        {
            message += "¡Excelente trabajo! Has completado el recorrido con éxito.";
        }
        else
        {
            message += "No alcanzaste la puntuación necesaria. Intenta evitar más obstáculos, recoger más objetos y no tocar los bordes.";
        }
        
        return message;
    }
}
