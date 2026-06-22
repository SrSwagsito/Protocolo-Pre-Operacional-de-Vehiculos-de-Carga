using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform towerParent;

    public Transform SpawnBlock()
    {
        GameObject newBlock = Instantiate(
            blockPrefab,
            spawnPoint.position,
            Quaternion.identity,
            towerParent);

        return newBlock.transform;
    }

    public void RemoveBlock(Transform block)
    {
        if (block != null)
            Destroy(block.gameObject);
    }
}