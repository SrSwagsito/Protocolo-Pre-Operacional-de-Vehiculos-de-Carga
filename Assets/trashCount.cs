using UnityEngine;

public class trashCount : MonoBehaviour
{
    [SerializeField] private ManagerRecolection gameManager;

    private void OnCollisionEnter2D(Collision2D other)
    {
        // =====================================================
        // RESIDUO CORRECTO
        // =====================================================

        if (other.gameObject.CompareTag(gameObject.tag))
        {
            int scoreRandom = Random.Range(1, 10);

            gameManager.AddScore(scoreRandom);

            Debug.Log("Residuo correcto. +" + scoreRandom);

            Destroy(other.gameObject);
        }

        // =====================================================
        // RESIDUO INCORRECTO
        // =====================================================

        else
        {
            Debug.Log("Residuo incorrecto. Pierde una vida");

            gameManager.LoseLife();

            Destroy(other.gameObject);
        }
    }
}