using UnityEngine;

public class HookSwingController : MonoBehaviour
{
    [Header("Swing")]
    public float swingSpeed = 1.5f;
    public float swingAmplitude = 4f;

    private float _time;

    public Vector3 HookPosition => transform.position;

    private void Update()
    {
        _time += Time.deltaTime * swingSpeed;
        float x = Mathf.Sin(_time) * swingAmplitude;
        transform.localPosition = new Vector3(x, transform.localPosition.y, 0f);
    }

    public void SetActive(bool active) => enabled = active;
}