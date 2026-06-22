using UnityEngine;

public class Module4Manager : MonoBehaviour
{
    private enum GameState
    {
        Stacking,
        Rope,
        Completed,
        Failed
    }

    [SerializeField] private StackTowerController stackModule;
    [SerializeField] private CrossRopeController ropeController;

    private GameState state;

    private void Start()
    {
        state = GameState.Stacking;

        ropeController.gameObject.SetActive(true);

        stackModule.OnFail += HandleFail;
        stackModule.OnCompleted += HandleStackComplete;

        ropeController.OnSwingEnded += HandleRopeComplete;
    }

    public void HandleFail()
    {
        if (state == GameState.Failed) return;

        state = GameState.Failed;

        stackModule.enabled = false;
        ropeController.enabled = false;

        Debug.Log("MODULE FAILED");
    }

    private void HandleStackComplete()
    {
        if (state != GameState.Stacking) return;

        state = GameState.Rope;

        Debug.Log("STACK COMPLETE → START ROPE");

        ropeController.gameObject.SetActive(true);

        ropeController.StartSwinging();
    }


    private void HandleRopeComplete()
    {
        if (state != GameState.Rope) return;

        state = GameState.Completed;

        Debug.Log("MODULE COMPLETED");
    }
}