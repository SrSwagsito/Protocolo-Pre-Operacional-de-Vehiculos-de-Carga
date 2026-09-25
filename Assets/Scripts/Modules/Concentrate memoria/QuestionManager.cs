using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static System.Net.Mime.MediaTypeNames;

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
    [Header("Sprites botones")]
    [SerializeField] private Sprite spriteA;
    [SerializeField] private Sprite spriteB;
    [SerializeField] private Sprite spriteC;
    [SerializeField] private Sprite spriteD;

    [Header("Sprites Verdadero / Falso")]
    [SerializeField] private Sprite spriteVerdadero;
    [SerializeField] private Sprite spriteFalso;

    [Header("Imagen de los botones")]
    [SerializeField] private UnityEngine.UI.Image imageA;
    [SerializeField] private UnityEngine.UI.Image imageB;
    [SerializeField] private UnityEngine.UI.Image imageC;
    [SerializeField] private UnityEngine.UI.Image imageD;

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

        // ==========================================
        // PREGUNTA VERDADERO / FALSO
        // ==========================================

        if (currentQuestion.questionType == QuestionType.TrueFalse)
        {
            // Activar A y B
            buttonA.gameObject.SetActive(true);
            buttonB.gameObject.SetActive(true);

            // Desactivar C y D
            buttonC.gameObject.SetActive(false);
            buttonD.gameObject.SetActive(false);

            // Textos
            textA.text = "Verdadero";
            textB.text = "Falso";

            // Sprites
            imageA.sprite = spriteVerdadero;
            imageB.sprite = spriteFalso;

            // Interacción
            buttonA.interactable = true;
            buttonB.interactable = true;
        }

        // ==========================================
        // PREGUNTA DE OPCIÓN MÚLTIPLE
        // ==========================================

        else
        {
            // Activar los cuatro
            buttonA.gameObject.SetActive(true);
            buttonB.gameObject.SetActive(true);
            buttonC.gameObject.SetActive(true);
            buttonD.gameObject.SetActive(true);

            // Textos
            textA.text = currentQuestion.optionA;
            textB.text = currentQuestion.optionB;
            textC.text = currentQuestion.optionC;
            textD.text = currentQuestion.optionD;

            // Sprites
            imageA.sprite = spriteA;
            imageB.sprite = spriteB;
            imageC.sprite = spriteC;
            imageD.sprite = spriteD;

            // Interacción
            buttonA.interactable = true;
            buttonB.interactable = true;
            buttonC.interactable = true;
            buttonD.interactable = true;
        }
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
        Debug.Log("🟢 RESPUESTA PRESIONADA: " + answer);

        if (!answering)
        {
            Debug.LogWarning("⚠️ No se puede responder ahora.");
            return;
        }

        answering = false;

        bool correct = false;

        // Pregunta A/B/C/D
        if (currentQuestion.questionType == QuestionType.MultipleChoice)
        {
            correct = answer == currentQuestion.correctAnswer;
        }
        // Verdadero / Falso
        else
        {
            bool selectedTrue = answer == 0;

            correct = selectedTrue == currentQuestion.correctTrueFalse;
        }

        if (correct)
        {
            Debug.Log("✅ RESPUESTA CORRECTA");
        }
        else
        {
            Debug.Log("❌ RESPUESTA INCORRECTA");

            // Buscar automáticamente el ManagerRecolection
            if (ManagerRecolection.Instance != null)
            {
                ManagerRecolection.Instance.LoseLife();

                Debug.Log(
                    "❤️ VIDA PERDIDA. Vidas restantes: " +
                    ManagerRecolection.Instance.Health
                );
            }
            else
            {
                Debug.LogError(
                    "❌ ManagerRecolection.Instance es NULL"
                );
            }
        }

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