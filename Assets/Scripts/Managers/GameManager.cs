using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event System.Action<ModuleResult> OnModuleFinished;

    public GameState CurrentState { get; private set; }

    private List<ModuleResult> results = new()  ;

    //public string levelName;

    public string UserName { get; private set; }
    public string UserID { get; private set; }
    public Sprite UserAvatar { get; private set;}

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
        if (result == null)
        {
            Debug.LogError("No se puede registrar un resultado nulo.");
            return;
        }

        int index = results.FindIndex(
            r => r.moduleName == result.moduleName
        );

        if (index >= 0)
            results[index] = result;
        else
            results.Add(result);

        Debug.Log("Resultados guardados: " + results.Count);

        // Enviar el resultado a la API de Vercel.
        if (InterAseoApi.Instance != null)
        {
            InterAseoApi.Instance.EnviarResultado(
                UserName,
                UserID,
                result.moduleName,
                result.score,
                result.approved
            );
        }
        else
        {
            Debug.LogWarning(
                "InterAseoApi no está disponible. " +
                "El resultado se conserva en memoria, pero no se envió al servidor."
            );
        }

        // Mantener el comportamiento actual de la interfaz.
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

    public void StartEvaluation(string levelName)
    {
        ClearAllResults();
        ChangeState(GameState.IntroductionScene);

        StartCoroutine(SceneLoader.Instance.LoadSceneCoroutine(levelName));
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

    public void SetUserData(string name, string id, Sprite avatar = null)
    {
        UserName = name;
        UserID = id;
        UserAvatar = avatar;
        Debug.Log($"Usuario registrado: {UserName} - ID: {UserID}");
    }

}
