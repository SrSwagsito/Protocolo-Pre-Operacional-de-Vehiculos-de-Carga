using UnityEngine;

public class TrashCount : MonoBehaviour
{
    [SerializeField] private ManagerRecolection gameManager;

    private int scorePlayer = 0;

    private bool level2Unlocked;
    private bool level3Unlocked;
    private bool moduleCompleted;

    public int ScorePlayer
    {
        get => scorePlayer;
        set => scorePlayer = value;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Residuo correcto
        if (other.gameObject.CompareTag(gameObject.tag))
        {
            int scoreRandom = Random.Range(1, 10);

            ScorePlayer += scoreRandom;

            gameManager.score.text = "Puntos: " + ScorePlayer;

            Destroy(other.gameObject);

            CheckProgress();
        }
        else
        {
            Debug.Log("Incorrecto. Pierde una vida");

            gameManager.LoseLife();

            Destroy(other.gameObject);
        }
    }

    private void CheckProgress()
    {
        if (!level2Unlocked && ScorePlayer >= 10)
        {
            level2Unlocked = true;
            gameManager.NextLevel();
        }

        if (!level3Unlocked && ScorePlayer >= 20)
        {
            level3Unlocked = true;
            gameManager.Level3();
        }

        if (!moduleCompleted && ScorePlayer >= 40)
        {
            moduleCompleted = true;

            Debug.Log("Escena cargada");

            gameManager.NextModule();
        }
    }
}