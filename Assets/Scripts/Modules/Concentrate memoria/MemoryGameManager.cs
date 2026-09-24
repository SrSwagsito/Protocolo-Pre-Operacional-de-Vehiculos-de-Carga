using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryGameManager : MonoBehaviour
{
    [Header("Tablero")]
    [SerializeField] private Transform board;
    [SerializeField] private MemoryCard cardPrefab;

    [Header("Parejas")]
    [SerializeField] private MemoryPairData[] pairs;

    [Header("Configuración")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 5;

    [SerializeField] private int pointsPerPair = 10;

    [Header("Tiempo")]
    [SerializeField] private float timeToHide = 0.8f;

    [Header("Manager principal")]
    [SerializeField] private ManagerRecolection managerRecolection;
    [Header("Pregunta")]
    [SerializeField] private QuestionManager questionManager;   


    private List<MemoryCard> cards = new List<MemoryCard>();

    private MemoryCard firstCard;
    private MemoryCard secondCard;

    private bool checkingPair = false;

    private int pairsFound = 0;
    private int totalPairs;


    private void Start()
    {
        totalPairs = pairs.Length;

        CreateBoard();
    }


    // =========================================================
    // CREAR TABLERO
    // =========================================================

    private void CreateBoard()
    {
        if (pairs == null || pairs.Length == 0)
        {
            Debug.LogError("No hay imágenes configuradas para las parejas.");
            return;
        }

        int totalCards = rows * columns;

        if (totalCards != totalPairs * 2)
        {
            Debug.LogError(
                "El número de casillas no coincide con las parejas. " +
                "Para 10 parejas necesitas 20 casillas."
            );

            return;
        }

        List<CardData> deck = new List<CardData>();

        // Crear dos cartas de cada pareja
        for (int i = 0; i < pairs.Length; i++)
        {
            deck.Add(new CardData(
                i,
                pairs[i].sprite
            ));

            deck.Add(new CardData(
                i,
                pairs[i].sprite
            ));
        }

        // Mezclar
        Shuffle(deck);

        // Crear cartas
        for (int i = 0; i < deck.Count; i++)
        {
            MemoryCard newCard = Instantiate(
                cardPrefab,
                board
            );

            newCard.Setup(
                deck[i].pairID,
                deck[i].sprite,
                this
            );

            cards.Add(newCard);
        }

        Debug.Log(
            "Tablero creado: " +
            totalCards +
            " cartas / " +
            totalPairs +
            " parejas."
        );
    }


    // =========================================================
    // CARTA SELECCIONADA
    // =========================================================

    public void CardSelected(MemoryCard card)
    {
        if (checkingPair)
            return;

        if (card.IsMatched)
            return;

        // Primera carta
        if (firstCard == null)
        {
            firstCard = card;
            firstCard.Reveal();

            return;
        }

        // Evitar seleccionar la misma carta
        if (card == firstCard)
            return;

        // Segunda carta
        secondCard = card;
        secondCard.Reveal();

        StartCoroutine(CheckPair());
    }


    // =========================================================
    // COMPROBAR PAREJA
    // =========================================================

    private IEnumerator CheckPair()
    {
        checkingPair = true;

        // Esperar a que ambas cartas terminen de girar
        yield return new WaitForSeconds(0.45f);

        if (firstCard.PairID == secondCard.PairID)
        {
            Debug.Log("¡Pareja encontrada!");

            firstCard.SetMatched();
            secondCard.SetMatched();

            pairsFound++;

            // Puntos por encontrar la pareja
            if (managerRecolection != null)
            {
                managerRecolection.AddScore(pointsPerPair);
            }

            Debug.Log(
                "Parejas encontradas: " +
                pairsFound +
                "/" +
                totalPairs
            );

            // Obtener los datos de esta pareja
            MemoryPairData pair = pairs[firstCard.PairID];

            // Mostrar pregunta
            if (questionManager != null)
            {
                questionManager.ShowQuestion(pair);
            }
            else
            {
                Debug.LogWarning("QuestionManager no está asignado.");

                firstCard = null;
                secondCard = null;
                checkingPair = false;
            }
        }
        else
        {
            // =============================================
            // PAREJA INCORRECTA
            // =============================================

            Debug.Log("No son pareja.");

            yield return new WaitForSeconds(timeToHide);

            firstCard.Hide();
            secondCard.Hide();
        }

        firstCard = null;
        secondCard = null;

        checkingPair = false;
    }


    // =========================================================
    // TERMINAR JUEGO
    // =========================================================

    private void FinishGame()
    {
        Debug.Log("¡Todas las parejas encontradas!");

        // Bloquear las cartas
        foreach (MemoryCard card in cards)
        {
            card.SetInteractable(false);
        }

        // Finalizar utilizando TU ManagerRecolection
        if (managerRecolection != null)
        {
            managerRecolection.FinishInspection();
        }
        else
        {
            Debug.LogWarning(
                "No hay ManagerRecolection asignado."
            );
        }
    }


    // =========================================================
    // MEZCLAR
    // =========================================================

    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(
                i,
                list.Count
            );

            CardData temp = list[i];

            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }


    // =========================================================
    // DATOS DE CARTA
    // =========================================================

    private class CardData
    {
        public int pairID;
        public Sprite sprite;

        public CardData(int id, Sprite image)
        {
            pairID = id;
            sprite = image;
        }
    }
    public void QuestionFinished(bool correct)
    {
        if (correct)
        {
            Debug.Log("Pregunta respondida correctamente.");
        }
        else
        {
            Debug.Log("Pregunta respondida incorrectamente.");

            if (managerRecolection != null)
            {
                managerRecolection.LoseLife();
            }
        }

        firstCard = null;
        secondCard = null;
        checkingPair = false;

        if (pairsFound >= totalPairs)
        {
            FinishGame();
        }
    }
}