using UnityEngine;

public class trashCount : MonoBehaviour
{
    [SerializeField] private ManagerRecolection gameManager;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("limit"))
        {
            return;
        }
        // =====================================================
        // RESIDUO CORRECTO
        // =====================================================

        if (other.gameObject.CompareTag(gameObject.tag))
        {
            int scoreRandom = Random.Range(10, 15);

            gameManager.AddScore(scoreRandom);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("UI simple button");
            }

            Debug.Log("Residuo correcto. +" + scoreRandom);

            Destroy(other.gameObject);
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("Wrong choose check");
            }

            Debug.Log("Residuo incorrecto. Pierde una vida");

            gameManager.LoseLife();

            Destroy(other.gameObject);
        }
    }
}