using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text wordText;
    [SerializeField] private Image checkImage;

    private string word;

    public void Initialize(string newWord)
    {
        word = newWord.ToUpper();

        wordText.text = word;

        checkImage.gameObject.SetActive(false);
    }

    public void MarkAsFound()
    {
        wordText.fontStyle = FontStyles.Strikethrough;
        wordText.color = Color.gray;

        checkImage.gameObject.SetActive(true);
    }

    public string Word => word;
}