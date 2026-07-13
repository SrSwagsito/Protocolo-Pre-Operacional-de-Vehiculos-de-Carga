using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordSearchManager : MonoBehaviour
{
    [Header("Inicio del modulo")]
    [SerializeField] private TutorialPanel tutorialPopup;
    [SerializeField] private ProgressCounter progressCounter;
    [SerializeField] private InstructionsUI instructionsUI;
    [SerializeField] private WordListUI wordListUI;


    [Header("Configuración del tablero")]
    [SerializeField] private int rows = 10;
    [SerializeField] private int cols = 10;
    [SerializeField] private List<string> wordsToFind;
    [SerializeField] private int requiredWords;
    [SerializeField] private GameObject letterCellPrefab; // Prefab de celda
    [SerializeField] private Transform boardParent;       // Referencia al GridLayoutGroup

    private char[,] grid;
    private LetterCell[,] spawnedCells;


    private HashSet<string> foundWords =
    new HashSet<string>();

    private int correctWords = 0;
    private int incorrectWords = 0;
    private int score = 0;
    private bool moduleFinished = false;


    private void OnEnable()
    {

        if (tutorialPopup != null)
            tutorialPopup.OnTutorialClosed += StartInspection;
    }

    private void OnDisable()
    {

        if (tutorialPopup != null)
            tutorialPopup.OnTutorialClosed -= StartInspection;
    }

    void Start()
    {
        // Mostrar tutorial
        if (tutorialPopup != null)
        {
            tutorialPopup.ShowTutorial();
        }
        else
        {
            StartInspection();
        }


        bool success = false;

        while (!success)
        {
            success = GenerateBoard();
        }

        FillEmptySpaces();
        RenderBoard();
        wordListUI.Initialize(wordsToFind);
    }

    private void StartInspection()
    {
        instructionsUI.ShowInstruction("Arrastra los documentos necesarios a la zona de verificación.");
        UpdateProgressCounter();
    }

    private void UpdateProgressCounter()
    {
        if (progressCounter != null)
        {
            //progressCounter.UpdateProgress(correctWords, wordsToFind.Contains(wordsToFind.Count()));
        }
    }

    private void GenerateGrid()
    {
        grid = new char[rows, cols];
    }

    private bool CanPlaceWord(string word, int row, int col, Vector2Int dir)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int r = row + dir.y * i;
            int c = col + dir.x * i;

            // Verificamos límites
            if (r < 0 || r >= rows || c < 0 || c >= cols)
                return false;

            // Permitimos reutilizar letras si coinciden
            if (grid[r, c] != '\0' && grid[r, c] != word[i])
                return false;
        }
        return true;
    }

    private void PlaceWord(string word, int row, int col, Vector2Int dir)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int r = row + dir.y * i;
            int c = col + dir.x * i;
            grid[r, c] = word[i]; // Se reutiliza si ya coincide
        }
    }

    private void FillEmptySpaces()
    {
        System.Random rand = new System.Random();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] == '\0')
                {
                    grid[r, c] = (char)('A' + rand.Next(0, 26));
                }
            }
        }
    }

    private void RenderBoard()
{
    spawnedCells = new LetterCell[rows, cols];

    WordSelectionController controller =
        FindObjectOfType<WordSelectionController>();

    for (int r = 0; r < rows; r++)
    {
        for (int c = 0; c < cols; c++)
        {
            GameObject obj =
                Instantiate(letterCellPrefab, boardParent);

            LetterCell cell =
                obj.GetComponent<LetterCell>();

            cell.Initialize(
                grid[r, c],
                new Vector2Int(c, r),
                controller);

            spawnedCells[r, c] = cell;
        }
    }
}

    public List<string> GetWords()
    {
        return wordsToFind;
    }

    public bool IsWordFound(string found )
    {
        return foundWords.Contains(found.ToUpper());
    }

    public void RegisterFoundWord(string word)
    {
        word = word.ToUpper();

        if (foundWords.Contains(word))
            return;

        foundWords.Add(word);

        wordListUI.MarkFound(word);

        UpdateProgressCounter();

        if (foundWords.Count >= requiredWords)
        {
            FinishModule();
        }
    }

    public LetterCell GetCell(int row, int col)
    {
        return spawnedCells[row, col];
    }

    private Vector2Int GetRandomDirection()
    {
        Vector2Int[] dirs =
        {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1),
        new Vector2Int(1,1),
        new Vector2Int(-1,-1),
        new Vector2Int(1,-1),
        new Vector2Int(-1,1)
    };

        return dirs[Random.Range(0, dirs.Length)];
    }

    private bool GenerateBoard()
    {
        GenerateGrid();

        foreach (string word in wordsToFind)
        {
            bool placed = false;

            for (int attempt = 0; attempt < 500 && !placed; attempt++)
            {
                int row = Random.Range(0, rows);
                int col = Random.Range(0, cols);

                Vector2Int dir = GetRandomDirection();

                if (CanPlaceWord(word, row, col, dir))
                {
                    PlaceWord(word, row, col, dir);
                    placed = true;
                }
            }

            if (!placed)
            {
                return false; // ❌ fallo total → regenerar tablero
            }
        }

        return true; // ✅ todas las palabras entraron
    }

    public void FinishModule()
    {
        if (moduleFinished) return;

        correctWords = 0;
        incorrectWords = 0;
        score = 0;

        // Validar palabras encontradas
        foreach (string word in foundWords)
        {
            if (wordsToFind.Contains(word.ToUpper()))
            {
                correctWords++;
                score += 20; // ejemplo: 20 puntos por palabra correcta
            }
            else
            {
                incorrectWords++;
                score -= 10; // penalización
            }
        }

        moduleFinished = true;

        if (score < 0) score = 0;

        SendResult();
    }

    private void SendResult()
    {
        string detailMessage = GenerateDetailMessage();

        ModuleResult result = new ModuleResult
        {
            moduleName = "Sopa de Letras",
            approved = correctWords >= requiredWords && incorrectWords == 0,
            score = score,
            missedDamages = requiredWords - correctWords + incorrectWords,
            message = detailMessage
        };

        GameManager.Instance.RegisterModuleResult(result);
    }

    private string GenerateDetailMessage()
    {
        string message = $"Palabras correctas: {correctWords}/{requiredWords}\n";
        message += $"Palabras incorrectas añadidas: {incorrectWords}\n\n";

        if (correctWords >= requiredWords && incorrectWords == 0)
        {
            message += "¡Perfecto! Encontraste todas las palabras sin errores.";
        }
        else if (correctWords >= requiredWords)
        {
            message += "Encontraste todas las palabras, pero incluiste algunas incorrectas.";
        }
        else
        {
            int missing = requiredWords - correctWords;
            message += $"Te faltaron {missing} palabra(s) necesarias. Revisa la lista de palabras requeridas.";
        }

        return message;
    }
}
