using UnityEngine;

public class DamagedZone : MonoBehaviour
{
    public bool isDamaged;
    private bool reported;

    public void Report(ExternalInspectionManager manager)
    {
        if (reported) return;

        reported = true;

        if (isDamaged)
            manager.RegisterCorrectDamage(this);
        //else
        //    manager.RegisterWrongReport();
    }
}
