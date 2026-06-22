using System;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    //[SerializeField] Animator animator;

    public event Action<DoorController> OnWarningDoor;
    public event Action<DoorController> OnOpenedDoor;


    public void OpenDoor()
    {

        Debug.Log("Se abrio la puerta perdio");
        //animator.SetTrigger("Open");
        OnOpenedDoor?.Invoke(this);
    }

    public void WarningDoor()
    {
        Debug.Log("puerta a punto de abrir");
       //animator.SetTrigger("Warning");
        OnWarningDoor?.Invoke(this);
    }

    public void CloseDoor()
    {
        Debug.Log("Puerta cerrada");
        //animator.SetTrigger("Close");
    }
}
