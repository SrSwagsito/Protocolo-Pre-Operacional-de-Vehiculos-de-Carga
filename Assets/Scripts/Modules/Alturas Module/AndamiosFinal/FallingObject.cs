using UnityEngine;

public class FallingObstacle : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 5f;

    private void Update()
    {
        transform.Translate(
            Vector3.down *
            fallSpeed *
            Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RopePlayerController player =
            other.GetComponent<RopePlayerController>();

        if (player != null)
        {
            player.HitObstacle();
        }

        Destroy(gameObject);
    }
}