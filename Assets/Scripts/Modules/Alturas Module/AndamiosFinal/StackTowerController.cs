using System;
using UnityEngine;

public class StackTowerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HookSwing hook;

    [SerializeField] private StackLandingZone landingZone;

    [SerializeField] private GameObject blockPrefab;

    [SerializeField] private Transform blockParent;

    [SerializeField] private float failOffset = 1.1f;
    [SerializeField] private float perfectOffset = 0.25f;
    [SerializeField] private float snapDistance = 0.45f;

    private StackBlock lastPlacedBlock;

    [Header("Gameplay")]
    [SerializeField] private int blocksRequired = 5;

    public float FailOffset => failOffset;
    public float PerfectOffset => perfectOffset;
    public float SnapDistance => snapDistance;

    public StackBlock LastPlacedBlock => lastPlacedBlock;

    public event Action<bool> OnBlockPlaced;

    public event Action OnCompleted;

    public event Action OnFail;

    private StackBlock currentBlock;

    private int placedBlocks;

    private bool finished;

    private void Start()
    {
        SpawnBlock();
    }

    void SpawnBlock()
    {
        if (finished)
            return;

        GameObject obj =
            Instantiate(
                blockPrefab,
                hook.Hook.position,
                Quaternion.identity,
                blockParent);

        currentBlock =
            obj.GetComponent<StackBlock>();

        currentBlock.Hold(hook.Hook);

        landingZone.Initialize(this);
    }

    public void DropCurrentBlock()
    {
        if (finished)
            return;

        if (currentBlock == null)
            return;

        currentBlock.Release();
    }

    public void NotifyPlaced(StackBlock block, bool perfect)
    {
        lastPlacedBlock = block;

        placedBlocks++;

        OnBlockPlaced?.Invoke(perfect);

        if (placedBlocks >= blocksRequired)
        {
            finished = true;

            OnCompleted?.Invoke();

            return;
        }
            
        Invoke(nameof(SpawnBlock), 0.4f);
    }

    public void NotifyFail()
    {
        finished = true;

        OnFail?.Invoke();
    }
}