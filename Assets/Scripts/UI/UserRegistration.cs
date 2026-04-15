using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UserRegistration : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI errorMessageText; // Un texto para mostrar el error
    [SerializeField] private GameObject registrationPanel;

    private void Start()
    {
        if (errorMessageText != null) errorMessageText.text = "";
        startButton.onClick.AddListener(SaveAndStart);
    }

    public void SaveAndStart()
    {
        string userName = nameInputField.text.Trim();
        string userID = idInputField.text.Trim();

        // 1. Validaciones de Nombre
        if (userName.Length < 3)
        {
            ShowError("El nombre es demasiado corto.");
            return;
        }
        if (userName.Length > 50)
        {
            ShowError("El nombre es demasiado largo.");
            return;
        }

        // 2. Validaciones de Cédula (ID)
        // Verificamos longitud
        if (userID.Length < 5)
        {
            ShowError("El documento es demasiado corto.");
            return;
        }
        if (userID.Length > 15)
        {
            ShowError("La documento es demasiado largo.");
            return;
        }

        // 3. Validación extra: ¿Es un número? (Opcional si el InputField ya es numérico)
        if (!long.TryParse(userID, out _))
        {
            ShowError("La cédula debe contener solo números.");
            return;
        }

        // Si pasa todas las validaciones
        Success(userName, userID);
    }

    private void ShowError(string message)
    {
        Debug.LogWarning(message);
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.color = Color.red;
        }
    }

    public void Success(string name, string id)
    {
        if (errorMessageText != null) errorMessageText.text = "";

        GameManager.Instance.SetUserData(name, id);
        registrationPanel.SetActive(false);
        GameManager.Instance.StartEvaluation();
    }
}   