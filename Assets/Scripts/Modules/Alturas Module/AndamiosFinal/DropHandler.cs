using UnityEngine;
using UnityEngine.EventSystems;

public class StackInputController :
    MonoBehaviour,
    IPointerDownHandler
{
    [SerializeField]
    private StackTowerController tower;

    public void OnPointerDown(PointerEventData eventData)
    {
        tower.DropCurrentBlock();
    }

#if UNITY_EDITOR || UNITY_STANDALONE

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tower.DropCurrentBlock();
        }
    }

#endif
}