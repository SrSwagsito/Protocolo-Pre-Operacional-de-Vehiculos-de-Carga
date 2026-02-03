using UnityEngine;

public class ClicHandler : MonoBehaviour
{
    private Camera cam;
    public ExternalInspectionManager inspectionManager;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(
                cam.ScreenToWorldPoint(Input.mousePosition),
                Vector2.zero
            );

            if (hit.collider == null) return;

            DamagedZone zone = hit.collider.GetComponent<DamagedZone>();
            if (zone != null)
            {
                zone.Report(inspectionManager);
            }
        }
    }
}
