using UnityEngine;

public class Module4Manager : MonoBehaviour
{
    [Header("Phase 1")]
    public StackTowerController towerController;

    [Header("Phase 2")]
    public RopePlayerController ropePlayer;
    public ObstacleSpawner obstacleSpawner;
    public RopeGoal ropeGoal;

    [Header("Module Config")]
    public string moduleName = "Module4_Andamio";

    private int _score;
    private int _mistakes;
    private ModuleState _state;

    private enum ModuleState { Idle, Stacking, Rope, Completed, Failed }

    private void Start()
    {
        towerController.OnBlockPlaced += HandleBlockPlaced;
        towerController.OnCompleted += HandleStackCompleted;
        towerController.OnFail += HandleFail;

        ropePlayer.OnSwingEnded += HandleSwingEnded;
        ropePlayer.OnFail += HandleFail;

        ropeGoal.Initialize(ropePlayer);

        StartStacking();
    }

    private void StartStacking()
    {
        _state = ModuleState.Stacking;
        towerController.StartPhase();
    }

    private void StartRope()
    {
        _state = ModuleState.Rope;
        obstacleSpawner.StartSpawning();
        ropePlayer.EnableMovement();
        ropePlayer.SetDirection(1f);
    }

    private void HandleBlockPlaced(bool perfect)
    {
        if (perfect) _score += 10;
        else _score += 5;
    }

    private void HandleStackCompleted()
    {
        StartRope();
    }

    private void HandleSwingEnded()
    {
        obstacleSpawner.StopSpawning();
        _state = ModuleState.Completed;
        RegisterResult(true, "Módulo completado exitosamente.");
    }

    private void HandleFail()
    {
        obstacleSpawner.StopSpawning();
        ropePlayer.DisableMovement();
        towerController.StopPhase();
        _mistakes++;
        _state = ModuleState.Failed;
        RegisterResult(false, "El módulo ha fallado.");
    }

    private void RegisterResult(bool approved, string message)
    {
        var result = new ModuleResult
        {
            moduleName = moduleName,
            approved = approved,
            score = _score,
            missedDamages = _mistakes,
            message = message
        };
        GameManager.Instance.RegisterModuleResult(result);
    }
}