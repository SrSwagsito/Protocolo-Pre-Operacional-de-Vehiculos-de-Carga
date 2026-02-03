using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event System.Action<ModuleResult> OnModuleFinished;

    public GameState CurrentState { get; private set; }

    private List<ModuleResult> results = new()  ;

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

        //switch (CurrentState)
        //{
        //    case GameState.InitialMenu:
        //        SceneLoader.LoadScene("InitialMenu");
        //        break;
        //    case GameState.IntroductionScene:
        //        SceneLoader.Instance.LoadScene("IntroductionScene");
        //        break;
        //    case GameState.Module1:
        //        SceneLoader.Instance.LoadScene("Module1");
        //        break;
        //    case GameState.Results1:
        //        SceneLoader.Instance.LoadScene("Results1");
        //        break;
        //    case GameState.Module2:
        //        SceneLoader.Instance.LoadScene("Module2");
        //        break;
        //    case GameState.Results2:
        //        SceneLoader.Instance.LoadScene("Results2");
        //        break;
        //    case GameState.Module3:
        //        SceneLoader.Instance.LoadScene("Module3");
        //        break;
        //    case GameState.Results3:
        //        SceneLoader.Instance.LoadScene("Results3");
        //        break;
        //    case GameState.FinalResults:
        //        SceneLoader.Instance.LoadScene("FinalResults");
        //        break;
        //}
    }

    public void StartEvaluation()
    {
        ChangeState(GameState.IntroductionScene);    
    }   
}
