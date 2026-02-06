using UnityEngine;

namespace eeGames.Widget
{
    public class LoadMainMenuWidget : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            // Show MainMenu Widget
            WidgetManager.Instance.Push(WidgetName.MainMenu);
        }
    }
}