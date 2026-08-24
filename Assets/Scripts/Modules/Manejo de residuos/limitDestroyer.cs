using System;
using UnityEngine;

public class limitDestroyer : MonoBehaviour
{
   [SerializeField] TrashSpawner trash;

    private void OnTriggerEnter2D(Collider2D other)
    {

        trash.TrashDestroyed();
        
    }
}
