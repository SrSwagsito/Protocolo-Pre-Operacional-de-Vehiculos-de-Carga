using UnityEngine;
using eeGames.Widget;

namespace eeGames
{
    public class SettingButton : MonoBehaviour
    {
        public Actor ToggleButtonActor;
        public void OnToggleSet(bool value)
        {
            if (value)
            {
                ToggleButtonActor.PerformActing();

            }
            else
            {
                ToggleButtonActor.PerformReverseActing();

            }
        }
    }
}

