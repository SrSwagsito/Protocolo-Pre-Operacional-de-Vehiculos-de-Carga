
using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Controla la Fase 1: construcción del andamio.
///
/// Cambios respecto a la versión anterior:
/// - Integra LifeSystem: un fallo descuenta una vida, no termina el módulo
///   inmediatamente. Solo dispara OnFail cuando LifeSystem queda vacío.
/// - Al fallar, el bloque hace FadeAndDisappear() antes de que aparezca
///   el siguiente; una corrutina bloquea el input durante ese tiempo.
/// - blocksRequired por defecto = 4 (configurable en Inspector).
/// </summary>
public class StackTowerController : MonoBehaviour
{
    [Header("Referencias")]
    public HookSwingController hookController;
    public StackInputController inputController;
    public GameObject blockPrefab;
    public Transform  spawnParent;

    [Header("Configuración Torre")]
    [Tooltip("Número de bloques necesarios para completar la fase.")]
    public int   blocksRequired = 4;
    public float landTolerance  = 0.6f;
    public float snapStrength   = 0.4f;
    public float perfectOffset  = 0.15f;
    public float failOffset     = 1.2f;

    // ─── Eventos ──────────────────────────────────────────────────────────────
    public event Action<bool> OnBlockPlaced; // bool = fue perfecto
    public event Action       OnCompleted;
    public event Action       OnFail;

    // ─── Estado interno ───────────────────────────────────────────────────────
    private StackBlock _currentBlock;
    private Vector3    _nextSpawnPosition;
    private int        _placedCount;
    private bool       _active;
    private bool       _waitingForFade;   // impide soltar otro bloque mientras dura el fade
    private LifeSystemFinal _lifeSystem;

    private void Awake()
    {
        _nextSpawnPosition = spawnParent.position;
    }

    // ─── Inicialización ───────────────────────────────────────────────────────

    /// <summary>Llamar desde Module4Manager antes de StartPhase().</summary>
    public void Initialize(LifeSystemFinal lifeSystem)
    {
        _lifeSystem = lifeSystem;
        _lifeSystem.OnLivesEmpty += HandleLivesEmpty;
    }

    // ─── Control de fase ─────────────────────────────────────────────────────

    public void StartPhase()
    {
        if (_active)
            return;

        _active = true;

        _waitingForFade = false;
        _placedCount   = 0;
        _nextSpawnPosition = spawnParent.position;

        inputController.OnDropInput += DropBlock;
        inputController.SetActive(true);
        hookController.SetActive(true);
        SpawnBlock();
    }

    public void StopPhase()
    {
        _active = false;
        inputController.OnDropInput -= DropBlock;
        inputController.SetActive(false);
        hookController.SetActive(false);
    }

    // ─── Flujo de bloques ─────────────────────────────────────────────────────

    private void SpawnBlock()
    {

        Debug.Log("SpawnBlock");


        var go = Instantiate(blockPrefab);
        _currentBlock = go.GetComponent<StackBlock>();
        _currentBlock.AttachToHook(hookController.transform);
    }

    public void DropBlock()
    {
        Debug.Log("DropBlock");

        if (_currentBlock == null || !_active || _waitingForFade) return;
        _currentBlock.Release();
        StartCoroutine(WaitForLanding(_currentBlock));
        _currentBlock = null;
    }

    private IEnumerator WaitForLanding(StackBlock block)
    {
        inputController.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        var rb      = block.GetComponent<Rigidbody2D>();
        float timeout = 3f;
        while (rb.linearVelocity.magnitude > 0.05f && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        EvaluateLanding(block);
    }

    private void EvaluateLanding(StackBlock block)
    {
        float offset = Mathf.Abs(block.transform.position.x - _nextSpawnPosition.x);

        // ── Fallo: bloque demasiado desalineado ───────────────────────────────
        if (offset > failOffset)
        {
            block.Freeze();
            _waitingForFade = true;
            _lifeSystem?.LoseLife();

            // El bloque se desvanece; cuando termina, si aún hay fase activa
            // y quedan vidas, generamos el siguiente bloque.
            block.FadeAndDisappear(() =>
            {
                _waitingForFade = false;
                if (_active)
                {
                    inputController.SetActive(true);
                    SpawnBlock();
                }
            });
            return;
        }

        // ── Snap assist ───────────────────────────────────────────────────────
        if (offset < landTolerance)
        {
            var pos = block.transform.position;
            pos.x               = Mathf.Lerp(pos.x, _nextSpawnPosition.x, snapStrength);
            block.transform.position = pos;
        }

        block.Freeze();

        bool perfect = offset <= perfectOffset;
        OnBlockPlaced?.Invoke(perfect);

        var collider = block.GetComponent<BoxCollider2D>();

        _nextSpawnPosition =
            new Vector2(
                block.transform.position.x,
                block.GetComponent<BoxCollider2D>().bounds.max.y);

        _placedCount++;

        if (_placedCount >= blocksRequired)
        {
            StopPhase();
            OnCompleted?.Invoke();
        }
        else
        {
            inputController.SetActive(true);
            SpawnBlock();
        }
    }

    // ─── Respuesta a LifeSystem ───────────────────────────────────────────────

    private void HandleLivesEmpty()
    {
        // Las vidas se agotaron durante un fade. Detenemos todo y notificamos.
        StopPhase();
        OnFail?.Invoke();
    }

    private void OnDestroy()
    {
        if (_lifeSystem != null)
            _lifeSystem.OnLivesEmpty -= HandleLivesEmpty;
    }
}
