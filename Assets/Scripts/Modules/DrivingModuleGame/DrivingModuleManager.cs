using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    [SerializeField] private TruckContorller truck;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private ProgressCounter progressCounter;
    [SerializeField] private TutorialPanel tutorial;
    [SerializeField] private InstructionsUI instructionsUI;

    [Header("Configuration")]

    [SerializeField] float levelDuration;
    [SerializeField] bool isChallengeMode = false;
    
    private int score = 0;
    private int obstacleHits = 0;
    private float timer;
    bool moduleFinished = false;

    private void OnEnable()
    {
        truck.OnOutOfLane += OnLanePenalty;
       truck.OnCollectibleHit += AddPoints;
        truck.OnObstacleHit += RestPoints;
        if ((tutorial != null))
        {
            tutorial.OnTutorialClosed += StartDriving;   
        }
    }

    private void OnDisable()
    {

        truck.OnOutOfLane -= OnLanePenalty;
        truck.OnCollectibleHit -= AddPoints;
        truck.OnObstacleHit -= RestPoints;
        if ((tutorial != null))
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
        instructionsUI.ShowInstruction("Conduce el camión y evita los obstáculos. ¡Recoge los objetos para ganar puntos!");
        spawnManager.SetDifficulty(isChallengeMode);
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

    private void RestPoints(int points)
    {
        obstacleHits++;
        score -= points;

        if (points >= 0)
        {
            points = 0;
        }

        progressCounter.UpdateProgress(score, 1000);
    }

    private void OnLanePenalty(int penalty)
    {
        score -= penalty;
        if (penalty >= 0)
        {
            penalty = 0;
        }
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
            missedDamages = obstacleHits,
            message = GenerateFinalMessage()
        };

        GameManager.Instance.RegisterModuleResult(result);
    }

   private string GenerateFinalMessage()
    {
        string message = $"Puntuación final: {score}\n";
        message += $"Obstáculos golpeados: {obstacleHits}\n\n";
        if (score >= 1000)
        {
            message += "¡Excelente trabajo! Has completado el recorrido con éxito.";
        }
        else
        {
            message += "No alcanzaste la puntuación necesaria. Intenta evitar más obstáculos y recoger más objetos la próxima vez.";
        }
        return message;
    }
}
