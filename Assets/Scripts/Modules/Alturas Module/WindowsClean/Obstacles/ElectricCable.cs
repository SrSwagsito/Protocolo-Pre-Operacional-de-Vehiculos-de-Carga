using System;
using UnityEngine;

public class ElectricCable : Obstacle
{

    // [SerializeField] int damage = 1;

    public override void Hit(PlayerHealth player)
    {
        base.Hit(player);
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
