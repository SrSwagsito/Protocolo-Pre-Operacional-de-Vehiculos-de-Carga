using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Prefab de cada bloque del andamio.
/// Novedad: FadeAndDisappear() — anima la desaparición del bloque
/// cuando falla antes de generar el siguiente.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class StackBlock : MonoBehaviour
{
    [Header("Fade al fallar")]
    [Tooltip("Duración del desvanecimiento antes de destruirse.")]
    public float fadeDuration = 0.6f;

    private Rigidbody2D    _rb;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>Fija el bloque al gancho (cinemático, hijo del transform).</summary>
    public void AttachToHook(Transform hook)
    {
        _rb.bodyType = RigidbodyType2D.Kinematic;
        transform.SetParent(hook);
        transform.localPosition = Vector2.zero;
        SetAlpha(1f);
    }

    /// <summary>Libera el bloque para que caiga con físicas.</summary>
    public void Release()
    {
        transform.SetParent(null);
        _rb.bodyType = RigidbodyType2D.Dynamic;
    }

    /// <summary>Detiene completamente el bloque (ya aterrizó correctamente).</summary>
    public void Freeze()
    {
        _rb.linearVelocity  = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.bodyType        = RigidbodyType2D.Kinematic;
    }

    /// <summary>
    /// Anima el desvanecimiento y destruye el objeto al terminar.
    /// onComplete se invoca justo antes del Destroy para que
    /// StackTowerController pueda continuar.
    /// </summary>
    public void FadeAndDisappear(Action onComplete = null)
    {
        StartCoroutine(FadeCoroutine(onComplete));
    }

    // ─── Privados ─────────────────────────────────────────────────────────────

    private IEnumerator FadeCoroutine(Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        onComplete?.Invoke();
        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        if (_sr == null) return;
        var c = _sr.color;
        c.a      = alpha;
        _sr.color = c;
    }
}
