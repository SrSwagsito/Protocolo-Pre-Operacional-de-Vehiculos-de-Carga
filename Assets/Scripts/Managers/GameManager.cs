using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event System.Action<ModuleResult> OnModuleFinished;

    public GameState CurrentState { get; private set; }

    private List<ModuleResult> results = new()  ;

    public string UserName { get; private set; }
    public string UserID { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AudioManager.Instance.PlayMusic("Musica modulo");
        ChangeState(GameState.InitialMenu);
    }

    public void RegisterModuleResult(ModuleResult result)
    {
        results.Add(result);
        OnModuleFinished?.Invoke(result);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        HandleState();
    }
        

    private void HandleState()
    {
        Debug.Log("Estado actual: " + CurrentState);
    }

    public void StartEvaluation()
    {
        ChangeState(GameState.IntroductionScene);

        StartCoroutine(SceneLoader.Instance.LoadSceneCoroutine("IntroductionScene"));
    }

    public List<ModuleResult> GetAllModuleResults()
    {
        return new List<ModuleResult>(results);
    }

    public void ClearAllResults()
    {
        results.Clear();
    }

    public int GetAllScore()
    {
        int total = 0;
        foreach (var moduleResults in results)
        {
            total += moduleResults.score;
        }

        return total;
    }

    public int GetApprovedModulesCount()
    {
        int count = 0;
        foreach (var moduleResults in results)
        {
            if (moduleResults.approved)
                count++;
        }
        return count;
    }

    public bool AllModulesApproved()
    {
        if (results.Count == 0)
            return false;

        foreach (var moduleResult in results)
        {
            if (!moduleResult.approved)
                return false;
        }
        return true;
    }

    public void SetUserData(string name, string id)
    {
        UserName = name;
        UserID = id;
        Debug.Log($"Usuario registrado: {UserName} - ID: {UserID}");
    }

}
