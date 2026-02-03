    using UnityEngine;

public class IntroPanel : BaseUIPanel
{
    public void StartInstroduction  ()
    {
        GameManager.Instance.ChangeState(GameState.IntroductionScene);
    }
}
