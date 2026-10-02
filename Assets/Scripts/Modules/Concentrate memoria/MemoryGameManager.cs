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

    // Evita perder dos vidas por una misma pregunta
    private bool questionResultProcessed = false;

    private int pairsFound = 0;
    private int totalPairs;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        totalPairs = pairs.Length;

        // Conectar QuestionManager con este MemoryGameManager
        if (questionManager != null)
        {
            questionManager.SetMemoryGameManager(this);

            Debug.Log(
                "✅ QuestionManager conectado con MemoryGameManager."
            );
        }
        else
        {
            Debug.LogError(
                "❌ QuestionManager NO está asignado."
            );
        }

        // Buscar automáticamente ManagerRecolection
        if (managerRecolection == null)
        {
            managerRecolection = ManagerRecolection.Instance;
        }

        if (managerRecolection != null)
        {
            Debug.Log(
                "✅ ManagerRecolection conectado."
            );
        }
        else
        {
            Debug.LogError(
                "❌ ManagerRecolection NO encontrado."
            );
        }

        CreateBoard();
    }


    // =========================================================
    // CREAR TABLERO
    // =========================================================

    private void CreateBoard()
    {
        if (pairs == null || pairs.Length == 0)
        {
            Debug.LogError(
                "No hay imágenes configuradas para las parejas."
            );

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
            deck.Add(
                new CardData(
                    i,
                    pairs[i].sprite
                )
            );

            deck.Add(
                new CardData(
                    i,
                    pairs[i].sprite
                )
            );
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

        // =====================================================
        // PAREJA CORRECTA
        // =====================================================

        if (firstCard.PairID == secondCard.PairID)
        {
            Debug.Log("¡Pareja encontrada!");

            firstCard.SetMatched();
            secondCard.SetMatched();

            pairsFound++;

            Debug.Log(
                "Parejas encontradas: " +
                pairsFound +
                "/" +
                totalPairs
            );

            // Puntos por encontrar pareja
            if (managerRecolection != null)
            {
                managerRecolection.AddScore(pointsPerPair);
            }

            // Obtener datos de la pareja
            MemoryPairData pair = pairs[firstCard.PairID];

            // Mostrar pregunta
            if (questionManager != null)
            {
                Debug.Log(
                    "❓ Mostrando pregunta de la pareja."
                );

                // Reiniciar protección para esta nueva pregunta
                questionResultProcessed = false;

                questionManager.ShowQuestion(pair);

                // Esperar a que QuestionManager
                // llame a QuestionFinished()
                yield break;
            }
            else
            {
                Debug.LogError(
                    "❌ QuestionManager no está asignado."
                );

                firstCard = null;
                secondCard = null;
                checkingPair = false;
            }
        }

        // =====================================================
        // PAREJA INCORRECTA
        // =====================================================

        else
        {
            Debug.Log("❌ No son pareja.");

            yield return new WaitForSeconds(
                timeToHide
            );

            firstCard.Hide();
            secondCard.Hide();

            firstCard = null;
            secondCard = null;

            checkingPair = false;
        }
    }


    // =========================================================
    // TERMINAR JUEGO
    // =========================================================

    private void FinishGame()
    {
        Debug.Log(
            "================================="
        );

        Debug.Log(
            "🏆 ¡MEMORY GAME COMPLETADO!"
        );

        Debug.Log(
            "🏆 Parejas: " +
            pairsFound +
            "/" +
            totalPairs
        );

        // Bloquear cartas
        foreach (MemoryCard card in cards)
        {
            card.SetInteractable(false);
        }

        // Finalizar módulo
        if (managerRecolection != null)
        {
            Debug.Log(
                "📊 Puntaje final: " +
                managerRecolection.Score
            );

            Debug.Log(
                "📋 Finalizando módulo..."
            );

            managerRecolection.FinishInspection();
        }
        else
        {
            Debug.LogError(
                "❌ No se puede finalizar el módulo porque " +
                "ManagerRecolection es NULL."
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


    // =========================================================
    // PREGUNTA TERMINADA
    // =========================================================

    public void QuestionFinished(bool correct)
    {
        Debug.Log(
            "📋 QuestionFinished recibido. Correcta: " + correct
        );

        if (correct)
        {
            Debug.Log("✅ Pregunta respondida correctamente.");
        }
        else
        {
            Debug.Log("❌ Pregunta respondida incorrectamente.");
        }

        // Preparar siguiente turno
        firstCard = null;
        secondCard = null;
        checkingPair = false;

        Debug.Log(
            "📊 Parejas encontradas: " +
            pairsFound +
            "/" +
            totalPairs
        );

        // Comprobar si ya terminó el juego
        if (pairsFound >= totalPairs)
        {
            Debug.Log("🏆 ¡TODAS LAS PAREJAS COMPLETADAS!");

            FinishGame();
        }
    }
}