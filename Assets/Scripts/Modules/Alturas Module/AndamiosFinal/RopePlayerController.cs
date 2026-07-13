using UnityEngine;
using System;

public class RopePlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Respawn")]
    public int maxHits = 3;

    public event Action OnSwingEnded;
    public event Action OnFail;

    private float _direction;
    private bool _canMove;
    private Vector3 _checkpoint;
    private int _hitCount;

    public void EnableMovement()
    {
        _checkpoint = transform.position;
        _canMove = true;
    }

    public void DisableMovement() => _canMove = false;

    public void SetDirection(float dir) => _direction = dir;

    private void Update()
    {
        if (!_canMove) return;
        transform.Translate(Vector3.right * (_direction * moveSpeed * Time.deltaTime));
    }

    public void ReachGoal()
    {
        DisableMovement();
        OnSwingEnded?.Invoke();
    }

    public void HitObstacle()
    {
        _hitCount++;
        if (_hitCount >= maxHits)
        {
            DisableMovement();
            OnFail?.Invoke();
            return;
        }
        Respawn();
    }

    private void Respawn() => transform.position = _checkpoint;
}