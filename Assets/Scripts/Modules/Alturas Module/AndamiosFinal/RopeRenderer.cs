using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeRenderer : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private Transform hook;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
    }

    void LateUpdate()
    {
        line.SetPosition(0, pivot.position);
        line.SetPosition(1, hook.position);
    }
}