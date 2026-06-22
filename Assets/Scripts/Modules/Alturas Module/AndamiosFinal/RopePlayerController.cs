using System;
using UnityEngine;

public class RopePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    private bool canMove;

    public event Action OnReachedGoal;
    public event Action OnHitObstacle;

    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
    }

    public void SetDirection(float direction)
    {
        if (!canMove)
            return;

        transform.Translate(
            Vector3.right *
            direction *
            moveSpeed *
            Time.deltaTime);
    }

    public void ReachGoal()
    {
        OnReachedGoal?.Invoke();
    }

    public void HitObstacle()
    {
        OnHitObstacle?.Invoke();
    }
}