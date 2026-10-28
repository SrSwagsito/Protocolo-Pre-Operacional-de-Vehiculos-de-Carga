
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class InterAseoApi : MonoBehaviour
{
    public static InterAseoApi Instance { get; private set; }

    [Header("Configuración de la API")]
    [SerializeField]
    private string apiUrl =
        "https://ninteraseowebgl.vercel.app/api/registrar-resultado";

    [Serializable]
    private class ResultadoRequest
    {
        public string nombre;
        public string cedula;
        public string modulo;
        public int puntaje;
        public bool aprobado;
    }

    [Serializable]
    private class ApiResponse
    {
        public bool success;
        public string message;
        public string error;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void EnviarResultado(
        string nombre,
        string cedula,
        string modulo,
        int puntaje,
        bool aprobado)
    {
        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(cedula) ||
            string.IsNullOrWhiteSpace(modulo))
        {
            Debug.LogError(
                "InterAseo: faltan datos para enviar el resultado.");
            return;
        }

        if (puntaje < 0 || puntaje > 100)
        {
            Debug.LogError(
                "InterAseo: el puntaje debe estar entre 0 y 100.");
            return;
        }

        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            Debug.LogError("InterAseo: la URL de la API está vacía.");
            return;
        }

        ResultadoRequest datos = new ResultadoRequest
        {
            nombre = nombre.Trim(),
            cedula = cedula.Trim(),
            modulo = modulo.Trim(),
            puntaje = puntaje,
            aprobado = aprobado
        };

        StartCoroutine(EnviarCoroutine(datos));
    }

    private IEnumerator EnviarCoroutine(ResultadoRequest datos)
    {
        string json = JsonUtility.ToJson(datos);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request =
               new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 20;

            request.SetRequestHeader(
                "Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "InterAseo: no se pudo guardar el resultado. " +
                    request.error + " | " +
                    request.downloadHandler.text);
                yield break;
            }

            ApiResponse respuesta =
                JsonUtility.FromJson<ApiResponse>(
                    request.downloadHandler.text);

            if (respuesta != null && respuesta.success)
            {
                Debug.Log(
                    "InterAseo: resultado guardado correctamente. " +
                    datos.modulo);
            }
            else
            {
                Debug.LogError(
                    "InterAseo: la API no confirmó el guardado. " +
                    request.downloadHandler.text);
            }
        }
    }
}
