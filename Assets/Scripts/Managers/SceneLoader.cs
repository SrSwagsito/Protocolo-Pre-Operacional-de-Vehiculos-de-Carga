//using System.Collections;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class SceneLoader : MonoBehaviour
//{
//    //public static SceneLoader Instance { get; private set; }

//    //private void Awake()
//    //{
//    //    if (Instance != null && Instance != this)
//    //    {
//    //        Destroy(gameObject);
//    //        return;
//    //    }
//    //    Instance = this;
//    //    DontDestroyOnLoad(gameObject);
//    //}

//    public void LoadScene(string sceneName)
//    {
//        StartCoroutine(LoadSceneCoroutine(sceneName));
//    }

//    public void ReloadCurrentScene()
//    {
//        Scene currentScene = SceneManager.GetActiveScene();
//        SceneManager.LoadScene(currentScene.name);
//    }

//    public IEnumerator LoadSceneCoroutine(string sceneName)
//    {
//        yield return ScreenFader.Instance.FadeOut(0);
//        SceneManager.LoadScene(sceneName);
//        yield return null;

//        yield return ScreenFader.Instance.FadeIn(0);
//    }

//}


using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    public void ReloadCurrentScene()
    {
        StartCoroutine(LoadSceneCoroutine(SceneManager.GetActiveScene().name));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // Fade Out
        yield return ScreenFader.Instance.FadeOut(0.5f);

        // Carga asíncrona
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // Esperar a que termine de cargar
        while (!operation.isDone)
        {
            yield return null;
        }

        // Fade In
        yield return ScreenFader.Instance.FadeIn(0.5f);
    }
}