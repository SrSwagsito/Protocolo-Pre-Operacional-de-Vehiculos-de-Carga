using System.Collections.Generic;
using UnityEngine;

public class WordListUI : MonoBehaviour
{
    [SerializeField] private WordItemUI wordPrefab;
    [SerializeField] private Transform content;

    private Dictionary<string, WordItemUI> items =
        new Dictionary<string, WordItemUI>();

    public void Initialize(List<string> words)
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        items.Clear();

        foreach (string word in words)
        {
            WordItemUI item =
                Instantiate(wordPrefab, content);

            item.Initialize(word);

            items.Add(word.ToUpper(), item);
        }
    }

    public void MarkFound(string word)
    {
        word = word.ToUpper();

        if (items.TryGetValue(word, out WordItemUI item))
        {
            item.MarkAsFound();
        }
    }
}