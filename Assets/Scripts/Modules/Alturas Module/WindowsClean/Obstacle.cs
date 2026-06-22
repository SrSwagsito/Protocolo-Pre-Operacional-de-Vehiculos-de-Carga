using UnityEngine;

public abstract class Obstacle : MonoBehaviour
{
    [SerializeField] protected int damage = 1;

    public virtual void Hit(PlayerHealth player)
    {
        player.ReceiveDamage(damage);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player =
            other.GetComponent<PlayerHealth>();

        if (player == null)
            return;

        Hit(player);
    }
}
