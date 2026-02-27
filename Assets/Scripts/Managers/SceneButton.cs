using UnityEngine;

public class SceneButton : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private bool reloadCurrent;

    public void OnButtonPressed()
    {
        if (reloadCurrent)
        {
            SceneLoader.Reload();
        }
        else
        {
            SceneLoader.Load(sceneName);
        }
    }
}