using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneLoader.Load(sceneName);
    }
}