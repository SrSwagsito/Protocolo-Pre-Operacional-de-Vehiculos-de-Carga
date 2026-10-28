
using UnityEngine;

public class trashCount : MonoBehaviour
{
    [SerializeField] private ManagerRecolection gameManager;

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Ignorar los límites del escenario.
        if (other.gameObject.CompareTag("limit"))
            return;

        // Evitar procesar objetos que no sean residuos.
        if (other.gameObject.GetComponent<FallingObject>() == null)
            return;

        // Residuo correcto.
        if (other.gameObject.CompareTag(gameObject.tag))
        {
            int scoreRandom = 10;

            if (gameManager != null)
                gameManager.AddScore(scoreRandom);

            if (AudioManager.Instance != null)
                AudioManager.Instance.Play("UI simple button");

            Debug.Log("Residuo correcto. +" + scoreRandom);
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play("Wrong choose check");

            Debug.Log("Residuo incorrecto. Pierde una vida");

            if (gameManager != null)
                gameManager.LoseLife();
        }

        // Registrar la eliminación antes de destruir el residuo.
        FallingObject fallingObject =
            other.gameObject.GetComponent<FallingObject>();

        fallingObject.NotifyRemoved();

        Destroy(other.gameObject);
    }
}
