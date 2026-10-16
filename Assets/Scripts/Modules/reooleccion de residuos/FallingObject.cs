
using System.Collections;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    [Header("Tiempo en el piso")]
    [SerializeField] private float lifeTime = 5f;

    private ObjectDrop spawner;
    private bool hasTouchedFloor;
    private bool removalRegistered;

    private void Awake()
    {
        spawner = FindFirstObjectByType<ObjectDrop>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTouchedFloor || removalRegistered)
            return;

        if (collision.gameObject.CompareTag("floor"))
        {
            hasTouchedFloor = true;

            Debug.Log(
                name + " tocó el piso. Se destruirá en " +
                lifeTime + " segundos.");

            StartCoroutine(DestroyAfterTime());
        }
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);

        if (removalRegistered)
            yield break;

        Debug.Log(name + " cumplió su tiempo en el piso.");

        NotifyRemoved();

        Destroy(gameObject);
    }

    public void NotifyRemoved()
    {
        if (removalRegistered)
            return;

        removalRegistered = true;

        if (spawner != null)
        {
            spawner.RemoveObject();
        }
    }
}
