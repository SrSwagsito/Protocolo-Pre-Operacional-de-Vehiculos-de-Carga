using UnityEngine;

public class FallingObject : MonoBehaviour
{
    private float lifeTime = 5f;
    private void Start()
    {
        Destroy(gameObject, 26f);
    }
    private void OnBecameInvisible()
    {
        ObjectDrop spawner = FindFirstObjectByType<ObjectDrop>();

        if (spawner != null)
            spawner.RemoveObject();

        Destroy(gameObject);
    }

}
