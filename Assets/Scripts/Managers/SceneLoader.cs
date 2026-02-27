using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("SceneLoader: Singleton creado y persistente");
    }

    // Método público estático para fácil acceso desde cualquier lugar
    public static void Load(string sceneName)
    {
        if (Instance != null)
        {
            Instance.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("SceneLoader.Instance es null! Asegúrate de tener un SceneLoader en la primera escena.");
        }
    }

    // Método público estático para recargar
    public static void Reload()
    {
        if (Instance != null)
        {
            Instance.ReloadCurrentScene();
        }
        else
        {
            Debug.LogError("SceneLoader.Instance es null!");
        }
    }

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
        Debug.Log($"SceneLoader: Iniciando carga de {sceneName}");

        // 1. Fade Out (oscurecer pantalla)
        yield return ScreenFader.Instance.FadeOut(1f);

        Debug.Log($"SceneLoader: Fade out completo, cargando escena...");

        // 2. Cargar escena de forma asíncrona
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // 3. Esperar a que termine de cargar COMPLETAMENTE
        while (!operation.isDone)
        {
            Debug.Log($"SceneLoader: Progreso de carga: {operation.progress * 100}%");
            yield return null;
        }

        Debug.Log($"SceneLoader: Escena {sceneName} cargada completamente");

        // 4. Esperar un frame extra para que la escena se inicialice
        yield return new WaitForEndOfFrame();

        // 5. Fade In (aclarar pantalla)
        yield return ScreenFader.Instance.FadeIn(1f);

        Debug.Log($"SceneLoader: Fade in completo. Transición terminada.");
    }
}