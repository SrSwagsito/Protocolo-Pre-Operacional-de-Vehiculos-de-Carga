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

    [Header("Tiempo")]
    [SerializeField] private float timeToHide = 0.8f;

    [Header("Manager principal")]
    [SerializeField] private ManagerRecolection managerRecolection;

    [Header("Pregunta")]
    [SerializeField] private QuestionManager questionManager;


    // =========================================================
    // VARIABLES DEL JUEGO
    // =========================================================

    private List<MemoryCard> cards = new List<MemoryCard>();

    private MemoryCard firstCard;
    private MemoryCard secondCard;

    private bool checkingPair = false;

    private int pairsFound = 0;
    private int totalPairs;


    // =========================================================
    // VARIABLES DEL RESULTADO
    // =========================================================

    // Cantidad de preguntas que ya fueron respondidas
    private int questionsAnswered = 0;

    // Cantidad de preguntas respondidas correctamente
    private int correctAnswers = 0;

    // Puntaje final del módulo
    private int finalScore = 0;

    // Evita finalizar el módulo dos veces
    private bool gameFinished = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (pairs == null)
        {
            Debug.LogError("❌ El arreglo de parejas es NULL.");
            return;
        }

        totalPairs = pairs.Length;

        // -----------------------------------------------------
        // Conectar QuestionManager
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // Buscar ManagerRecolection automáticamente
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // Crear tablero
        // -----------------------------------------------------

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
                "❌ No hay imágenes configuradas para las parejas."
            );

            return;
        }


        int totalCards = rows * columns;


        if (totalCards != totalPairs * 2)
        {
            Debug.LogError(
                "❌ El número de casillas no coincide con las parejas.\n" +
                "Para " +
                totalPairs +
                " parejas necesitas " +
                (totalPairs * 2) +
                " casillas."
            );

            return;
        }


        List<CardData> deck = new List<CardData>();


        // =====================================================
        // CREAR DOS CARTAS DE CADA PAREJA
        // =====================================================

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


        // =====================================================
        // MEZCLAR
        // =====================================================

        Shuffle(deck);


        // =====================================================
        // CREAR CARTAS
        // =====================================================

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
            "🎴 Tablero creado: " +
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
        // Si estamos comprobando una pareja no permitir otra
        if (checkingPair)
            return;


        // Si la carta ya fue encontrada
        if (card.IsMatched)
            return;


        // =====================================================
        // PRIMERA CARTA
        // =====================================================

        if (firstCard == null)
        {
            firstCard = card;

            firstCard.Reveal();

            return;
        }


        // =====================================================
        // EVITAR SELECCIONAR LA MISMA CARTA
        // =====================================================

        if (card == firstCard)
            return;


        // =====================================================
        // SEGUNDA CARTA
        // =====================================================

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
            Debug.Log("🎉 ¡Pareja encontrada!");


            firstCard.SetMatched();
            secondCard.SetMatched();


            pairsFound++;


            Debug.Log(
                "🃏 Parejas encontradas: " +
                pairsFound +
                "/" +
                totalPairs
            );


            // =================================================
            // OBTENER DATOS DE LA PAREJA
            // =================================================

            MemoryPairData pair = pairs[firstCard.PairID];


            // =================================================
            // MOSTRAR PREGUNTA
            // =================================================

            if (questionManager != null)
            {
                Debug.Log(
                    "❓ Mostrando pregunta de la pareja."
                );


                questionManager.ShowQuestion(pair);


                // QuestionManager se encargará de avisarnos
                // cuando el jugador termine la pregunta.

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
    // PREGUNTA TERMINADA
    // =========================================================

    public void QuestionFinished(bool correct)
    {
        // Si el juego ya terminó no hacer nada
        if (gameFinished)
            return;


        Debug.Log(
            "📋 QuestionFinished recibido. Correcta: " +
            correct
        );


        // =====================================================
        // CONTAR PREGUNTA
        // =====================================================

        questionsAnswered++;


        // =====================================================
        // CONTAR CORRECTAS
        // =====================================================

        if (correct)
        {
            correctAnswers++;

            Debug.Log(
                "✅ Pregunta respondida correctamente."
            );
        }
        else
        {
            Debug.Log(
                "❌ Pregunta respondida incorrectamente."
            );
        }


        // =====================================================
        // CALCULAR PUNTAJE ACTUAL
        // =====================================================

        float percentage =
            ((float)correctAnswers / questionsAnswered) * 100f;


        int currentScore =
            Mathf.RoundToInt(percentage);


        Debug.Log(
            "📊 PROGRESO DEL MÓDULO\n" +
            "Preguntas respondidas: " +
            questionsAnswered +
            "\nCorrectas: " +
            correctAnswers +
            "\nIncorrectas: " +
            (questionsAnswered - correctAnswers) +
            "\nPuntaje actual: " +
            currentScore
        );


        // =====================================================
        // PREPARAR SIGUIENTE TURNO
        // =====================================================

        firstCard = null;
        secondCard = null;
        checkingPair = false;


        // =====================================================
        // ¿YA TERMINÓ TODO EL JUEGO?
        // =====================================================

        if (pairsFound >= totalPairs)
        {
            Debug.Log(
                "🏆 ¡TODAS LAS PAREJAS FUERON COMPLETADAS!"
            );


            FinishGame();
        }
    }


    // =========================================================
    // TERMINAR JUEGO
    // =========================================================

    private void FinishGame()
    {
        if (gameFinished)
            return;


        gameFinished = true;


        Debug.Log(
            "================================="
        );


        Debug.Log(
            "🏆 ¡MEMORY GAME COMPLETADO!"
        );


        Debug.Log(
            "🃏 Parejas encontradas: " +
            pairsFound +
            "/" +
            totalPairs
        );


        Debug.Log(
            "📋 Preguntas respondidas: " +
            questionsAnswered
        );


        Debug.Log(
            "✅ Preguntas correctas: " +
            correctAnswers
        );


        Debug.Log(
            "❌ Preguntas incorrectas: " +
            (questionsAnswered - correctAnswers)
        );


        // =====================================================
        // CALCULAR PORCENTAJE FINAL
        // =====================================================

        if (questionsAnswered > 0)
        {
            finalScore = Mathf.RoundToInt(
                ((float)correctAnswers /
                questionsAnswered) *
                100f
            );
        }
        else
        {
            finalScore = 0;
        }


        Debug.Log(
            "================================="
        );


        Debug.Log(
            "🏆 PUNTAJE FINAL: " +
            finalScore +
            "/100"
        );


        Debug.Log(
            "================================="
        );


        // =====================================================
        // BLOQUEAR CARTAS
        // =====================================================

        foreach (MemoryCard card in cards)
        {
            card.SetInteractable(false);
        }


        // =====================================================
        // GUARDAR RESULTADO EN MANAGERRECOLECTION
        // =====================================================

        if (managerRecolection != null)
        {
            // IMPORTANTE:
            // El Score ya NO representa puntos por parejas.
            // Ahora representa el porcentaje de preguntas correctas.

            managerRecolection.Score = finalScore;


            Debug.Log(
                "📊 Score enviado a ManagerRecolection: " +
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


        public CardData(
            int id,
            Sprite image
        )
        {
            pairID = id;
            sprite = image;
        }
    }
}