using UnityEngine;


public class HightlightController : MonoBehaviour
{
    [SerializeField] private GameObject highlightEffect;


    public void ShowHighlight(Transform target)
    {
        highlightEffect.SetActive(true);
        highlightEffect.transform.position = target.position;
    }


    public void HideHighlight()     
    {
        highlightEffect.SetActive(false);
    }
}