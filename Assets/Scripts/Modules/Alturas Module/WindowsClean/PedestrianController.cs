using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PedestrianController : MonoBehaviour
{

    public enum PedestrianState
    {
        Walking,
        Waiting,
        Leaving
    }

    [SerializeField] private DoorController selectedDoor;
    [SerializeField] private Button stopButton;

    public event Action PedestrianToLeaving;

    public bool waitingForStop;

    private PedestrianState currentState;

    private void Start()
    {
        currentState = PedestrianState.Walking;

        StartCoroutine(SafetyRoutine());
    }
    private IEnumerator SafetyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                UnityEngine.Random.Range(8f, 15f));

            TriggerWarning();
        }
    }

    private void TriggerWarning()
    {

        Debug.Log("Advertencia activada");
        currentState = PedestrianState.Waiting;

        selectedDoor.WarningDoor();

        stopButton.gameObject.SetActive(true);

        StartCoroutine(WarningCountdown());
    }


    private IEnumerator WarningCountdown()
    {

        Debug.Log("Contando para cruzar");
        yield return new WaitForSeconds(2f);

        if (currentState ==
            PedestrianState.Waiting)
        {
            Debug.Log("Cruzo, fin del juego.");
            TriggerCrossing();
        }
    }

    private void TriggerCrossing()
    {
        Debug.Log("Cruzando ahora");

        currentState = PedestrianState.Leaving;

        stopButton.gameObject.SetActive(false);

        selectedDoor.OpenDoor();

        PedestrianToLeaving?.Invoke();
    }

    public void StopPedestrian()
    {
        if (currentState != PedestrianState.Waiting)
            return;

        currentState = PedestrianState.Walking;

        stopButton.gameObject.SetActive(false);

        selectedDoor.CloseDoor();

        Debug.Log("Persona detenida");
        selectedDoor.CloseDoor();
    }

}
