using UnityEngine;

public class StackLandingZone : MonoBehaviour
{
    private StackTowerController tower;

    private StackBlock currentBlock;

    public void Initialize(StackTowerController controller)
    {
        tower = controller;
    }

    public void SetCurrentBlock(StackBlock block)
    {
        currentBlock = block;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentBlock == null)
            return;

        StackBlock block =
            other.GetComponent<StackBlock>();

        if (block == null)
            return;

        if (block != currentBlock)
            return;

        Validate(block);
    }

    void Validate(StackBlock block)
    {
        Transform reference =
            tower.LastPlacedBlock == null
            ? transform
            : tower.LastPlacedBlock.transform;

        float offset =
            Mathf.Abs(
                block.transform.position.x -
                reference.position.x);

        if (offset > tower.FailOffset)
        {
            tower.NotifyFail();
            return;
        }

        bool perfect =
            offset <= tower.PerfectOffset;

        ApplySnap(block, reference);

        block.Lock();

        tower.NotifyPlaced(block, perfect);
    }

    void ApplySnap(
        StackBlock block,
        Transform reference)
    {
        Vector3 pos =
            block.transform.position;

        float difference =
            pos.x - reference.position.x;

        if (Mathf.Abs(difference) <=
            tower.SnapDistance)
        {
            pos.x = reference.position.x;
        }

        pos.y =
            reference.position.y +
            GetHeight(reference);

        block.transform.position = pos;
    }

    float GetHeight(Transform reference)
    {
        Collider2D col =
            reference.GetComponent<Collider2D>();

        Collider2D blockCol =
            currentBlock.GetComponent<Collider2D>();

        return
            col.bounds.extents.y +
            blockCol.bounds.extents.y;
    }
}