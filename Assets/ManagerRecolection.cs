using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ManagerRecolection : MonoBehaviour
{
    public static Action<int> OnLivesChanged;
    public static ManagerRecolection Instance;

    public static Action<int> OnScoreAdded;
    public static Action OnGameOver;

    public int Score { get; private set; }
    public int Health { get; private set; }
    private Transform[] players;

    public Transform player;
    public Transform playerTwo;
    public BoxCollider2D[] triggers;

    [SerializeField] private int maxLives = 3;
    public int health { get; private set; }

    public TextMeshProUGUI score;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        Health = maxLives;
        Score = 0;

        players = new Transform[2];
        players[0] = playerTwo;
        players[1] = player;

        OnLivesChanged?.Invoke(Health);
        OnScoreAdded?.Invoke(Score);
    }
    private void CheckProgress()
    {
        if (Score >= 10)
        {
            NextLevel();
        }

        if (Score >= 20)
        {
            Level3();
        }

        if (Score >= 40)
        {
            NextModule();
        }
    }
    public void AddScore(int points)
    {
        Score += points;

        score.text = "Puntos: " + Score;

        OnScoreAdded?.Invoke(Score);

        CheckProgress();
    }


    public void NextLevel()
    {
        player.position = new Vector3(player.position.x, 5.2f, player.position.z);
        playerTwo.position = new Vector3(playerTwo.position.x, 5.2f, playerTwo.position.z);

        triggers[1].isTrigger = false;
    }

    public void Level3()
    {
        player.position = new Vector3(player.position.x, 12.58f, player.position.z);
        playerTwo.position = new Vector3(playerTwo.position.x, 12.58f, playerTwo.position.z);

        triggers[2].isTrigger = false;
    }

    public void NextModule()
    {
        SceneManager.LoadScene("parttrhee");
    }

    public void LoseLife()
    {
        health = Mathf.Max(0, health - 1);

        // Notifica a la UI
        OnLivesChanged?.Invoke(health);

        if (health <= 0)
        {
            SceneManager.LoadScene("modulo recoleccion");
        }
    }
}