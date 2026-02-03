using TMPro;
using UnityEngine;

public class ModuleResultPanel : BaseUIPanel
{
    [SerializeField] GameObject panelContainer;
    [SerializeField] GameObject nextModule;
    [SerializeField] GameObject retryModule;

    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text detailText;

    private void OnEnable()
    {
        GameManager.Instance.OnModuleFinished += ShowResult;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnModuleFinished -= ShowResult;
    }

    void ShowResult(ModuleResult result)
    {
        panelContainer.SetActive(true);
        FindAnyObjectByType<ClicHandler>().GetComponent<ClicHandler>().enabled = false;

        titleText.text = result.moduleName;
        scoreText.text = "TIenes una seguridad de: " + result.score + "/100";

        if (result.approved)
        {
            resultText.text = "APROBADO";
            resultText.color = Color.green;
            nextModule.SetActive(true);
        }
        else
        {
            resultText.text = "NO APROBADO";
            resultText.color = Color.red;
            retryModule.SetActive(true);
        }

        detailText.text = result.message;
    }
}
