using Unity.VisualScripting;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{   [Header("cantidad de spawn")]
    [SerializeField] private int totalTrash =20;
    private int generatedTrash=0;
    public static TrashSpawner Instance;
    [SerializeField] private GameObject[] trashPrefabs;
    [SerializeField] private Transform spawnPoint;

    private GameObject currentTrash;

    public GameObject CurrentTrash { get => currentTrash; set => currentTrash = value; }

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        spawnTrash();
    }

    public void spawnTrash()
    {
        if (generatedTrash >= totalTrash)
        {
            gameObject.SetActive(false);
            Debug.Log("se han spawneado todas las basuras");
            TrashInspectionManager.Instance.FinishInspection();
            return;
        }
        if (CurrentTrash != null)
            return;
        int randomIndex = Random.Range(0, trashPrefabs.Length);
        CurrentTrash = Instantiate(trashPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
        generatedTrash++;
    }
    public void TrashDestroyed()
    {
        Destroy(CurrentTrash); // ✅ destruye la basura, no el spawner
        CurrentTrash = null;
        spawnTrash();
    }

}
