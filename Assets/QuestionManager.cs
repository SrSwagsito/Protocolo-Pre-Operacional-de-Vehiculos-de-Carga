using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class QuestionManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject questionPanel;

    [Header("Texto de la pregunta")]
    [SerializeField] private TextMeshProUGUI questionText;

    [Header("Botones")]
    [SerializeField] private UnityEngine.UI.Button buttonA;
    [SerializeField] private UnityEngine.UI.Button buttonB;
    [SerializeField] private UnityEngine.UI.Button buttonC;
    [SerializeField] private UnityEngine.UI.Button buttonD;

    [Header("Texto de botones")]
    [SerializeField] private TextMeshProUGUI textA;
    [SerializeField] private TextMeshProUGUI textB;
    [SerializeField] private TextMeshProUGUI textC;
    [SerializeField] private TextMeshProUGUI textD;

    [Header("Animación")]
    [SerializeField] private float animationDuration = 0.25f;

    private MemoryGameManager memoryGameManager;

    private QuestionData currentQuestion;

    private bool answering = false;


    private void Start()
    {
        if (questionPanel != null)
            questionPanel.SetActive(false);
    }


    // =========================================================
    // CONFIGURAR MANAGER
    // =========================================================

    public void SetMemoryGameManager(MemoryGameManager manager)
    {
        memoryGameManager = manager;
    }


    // =========================================================
    // MOSTRAR PREGUNTA
    // =========================================================

    public void ShowQuestion(MemoryPairData pair)
    {
        if (pair == null)
        {
            Debug.LogWarning("La pareja recibida es null.");
            return;
        }

        if (pair.questions == null || pair.questions.Length == 0)
        {
            Debug.LogWarning("Esta pareja no tiene preguntas.");
            return;
        }

        // Elegir una de las 3 preguntas aleatoriamente
        int randomIndex = Random.Range(
            0,
            pair.questions.Length
        );

        currentQuestion = pair.questions[randomIndex];

        LoadQuestion();

        answering = true;

        questionPanel.SetActive(true);

        StartCoroutine(ShowPanelAnimation());
    }


    // =========================================================
    // CARGAR PREGUNTA
    // =========================================================

    private void LoadQuestion()
    {
        questionText.text = currentQuestion.question;

        if (currentQuestion.questionType == QuestionType.MultipleChoice)
        {
            // Mostrar A, B, C y D
            buttonA.gameObject.SetActive(true);
            buttonB.gameObject.SetActive(true);
            buttonC.gameObject.SetActive(true);
            buttonD.gameObject.SetActive(true);

            textA.text = currentQuestion.optionA;
            textB.text = currentQuestion.optionB;
            textC.text = currentQuestion.optionC;
            textD.text = currentQuestion.optionD;
        }
        else
        {
            // Mostrar solamente Verdadero y Falso
            buttonA.gameObject.SetActive(true);
            buttonB.gameObject.SetActive(true);

            buttonC.gameObject.SetActive(false);
            buttonD.gameObject.SetActive(false);

            textA.text = "Verdadero";
            textB.text = "Falso";
        }

        buttonA.interactable = true;
        buttonB.interactable = true;
        buttonC.interactable = true;
        buttonD.interactable = true;
    }


    // =========================================================
    // RESPUESTAS
    // =========================================================

    public void AnswerA()
    {
        CheckAnswer(0);
    }

    public void AnswerB()
    {
        CheckAnswer(1);
    }

    public void AnswerC()
    {
        CheckAnswer(2);
    }

    public void AnswerD()
    {
        CheckAnswer(3);
    }


    private void CheckAnswer(int answer)
    {
        if (!answering) return;

        answering = false;

        bool correct = false;

        if (currentQuestion.questionType == QuestionType.MultipleChoice)
        {
            // A = 0
            // B = 1
            // C = 2
            // D = 3

            correct = answer == currentQuestion.correctAnswer;
        }
        else
        {
            // A = Verdadero
            // B = Falso

            bool selectedTrue = answer == 0;

            correct = selectedTrue == currentQuestion.correctTrueFalse;
        }

        Debug.Log(correct ? "Respuesta correcta" : "Respuesta incorrecta");

        StartCoroutine(CloseQuestion(correct));
    }


    // =========================================================
    // CERRAR PREGUNTA
    // =========================================================

    private IEnumerator CloseQuestion(bool correct)
    {
        SetButtonsInteractable(false);

        // Esperar un momento para que se vea la respuesta
        yield return new WaitForSeconds(0.8f);

        yield return StartCoroutine(HidePanelAnimation());

        questionPanel.SetActive(false);

        // Avisar al MemoryGameManager
        if (memoryGameManager != null)
        {
            memoryGameManager.QuestionFinished(correct);
        }
    }


    // =========================================================
    // ACTIVAR / DESACTIVAR BOTONES
    // =========================================================

    private void SetButtonsInteractable(bool value)
    {
        buttonA.interactable = value;
        buttonB.interactable = value;
        buttonC.interactable = value;
        buttonD.interactable = value;
    }


    // =========================================================
    // ANIMACIÓN ENTRADA
    // =========================================================

    private IEnumerator ShowPanelAnimation()
    {
        RectTransform rect =
            questionPanel.GetComponent<RectTransform>();

        if (rect == null)
            yield break;

        Vector3 targetScale = Vector3.one;

        rect.localScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                elapsed / animationDuration;

            // Smooth
            t = Mathf.SmoothStep(0f, 1f, t);

            rect.localScale =
                Vector3.Lerp(
                    Vector3.zero,
                    targetScale,
                    t
                );

            yield return null;
        }

        rect.localScale = targetScale;
    }


    // =========================================================
    // ANIMACIÓN SALIDA
    // =========================================================

    private IEnumerator HidePanelAnimation()
    {
        RectTransform rect =
            questionPanel.GetComponent<RectTransform>();

        if (rect == null)
            yield break;

        Vector3 startScale = rect.localScale;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                elapsed / animationDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            rect.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    t
                );

            yield return null;
        }

        rect.localScale = Vector3.zero;
    }

}