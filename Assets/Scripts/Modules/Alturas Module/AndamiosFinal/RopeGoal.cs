using UnityEngine;

public class RopeGoal : MonoBehaviour
{
    private RopePlayerController _player;

    public void Initialize(RopePlayerController player) => _player = player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _player?.ReachGoal();
    }
}