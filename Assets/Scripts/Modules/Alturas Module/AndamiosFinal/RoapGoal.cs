using UnityEngine;

public class RopeGoal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        RopePlayerController player =
            other.GetComponent<RopePlayerController>();

        if (player == null)
            return;

        player.ReachGoal();
    }
}