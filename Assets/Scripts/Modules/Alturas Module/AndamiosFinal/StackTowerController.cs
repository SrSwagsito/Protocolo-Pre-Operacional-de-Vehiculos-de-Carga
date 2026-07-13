using UnityEngine;
using System;
using System.Collections;

public class StackTowerController : MonoBehaviour
{
    [Header("References")]
    public HookSwingController hookController;
    public StackInputController inputController;
    public GameObject blockPrefab;
    public Transform spawnParent;

    [Header("Tower Config")]
    public int blocksRequired = 5;
    public float landTolerance = 0.6f;
    public float snapStrength = 0.4f;
    public float perfectOffset = 0.15f;
    public float failOffset = 1.2f;

    public event Action<bool> OnBlockPlaced;
    public event Action OnCompleted;
    public event Action OnFail;

    private StackBlock _currentBlock;
    private Vector3 _nextSpawnPosition;
    private int _placedCount;
    private bool _active;

    private void Awake()
    {
        _nextSpawnPosition = spawnParent.position;
    }

    public void StartPhase()
    {
        _active = true;
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

    private void SpawnBlock()
    {
        var go = Instantiate(blockPrefab);
        _currentBlock = go.GetComponent<StackBlock>();
        _currentBlock.AttachToHook(hookController.transform);
    }

    public void DropBlock()
    {
        if (_currentBlock == null || !_active) return;
        _currentBlock.Release();
        StartCoroutine(WaitForLanding(_currentBlock));
        _currentBlock = null;
    }

    private IEnumerator WaitForLanding(StackBlock block)
    {
        inputController.SetActive(false);
        yield return new WaitForSeconds(0.2f);

        // Wait until block settles
        var rb = block.GetComponent<Rigidbody2D>();
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

        if (offset > failOffset)
        {
            block.Freeze();
            OnFail?.Invoke();
            return;
        }

        // Snap assist
        if (offset < landTolerance)
        {
            var pos = block.transform.position;
            pos.x = Mathf.Lerp(pos.x, _nextSpawnPosition.x, snapStrength);
            block.transform.position = pos;
        }

        block.Freeze();

        bool perfect = offset <= perfectOffset;
        OnBlockPlaced?.Invoke(perfect);

        _nextSpawnPosition = block.transform.position + Vector3.up * blockPrefab.transform.localScale.y;
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
}