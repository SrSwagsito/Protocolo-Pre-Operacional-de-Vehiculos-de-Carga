using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class VehiclePartSlot
    {
        public Transform partTransform;
        public GameObject fuctionalPart;
        public GameObject damagedPart;

        //[Range(0, 1)] public float damageProbability;
    }

    [SerializeField] private int totalPartsDamaged = 10;

    public int TotalDamagedParts => totalPartsDamaged;

    public VehiclePartSlot[] vehiclePartSlots;
    public ExternalInspectionManager inspectionManager;

    private void Start()
    {
        SpawnParts();
    }

    void SpawnParts()
    {
        List<VehiclePartSlot> availableParts = new List<VehiclePartSlot>(vehiclePartSlots);
        List<VehiclePartSlot> damagedParts = new List<VehiclePartSlot>();

        for(int i = 0; i < TotalDamagedParts; i++)
        {
            int index = Random.Range(0, availableParts.Count);
            damagedParts.Add(availableParts[index]);
            availableParts.RemoveAt(index);
        }


        foreach (var part in vehiclePartSlots)
        {
            bool isDamaged = damagedParts.Contains(part);
            GameObject prefab = isDamaged ? part.damagedPart : part.fuctionalPart;
            GameObject instance = Instantiate(prefab, part.partTransform.position, part.partTransform.rotation, part.partTransform);

            DamagedZone zone = instance.GetComponent<DamagedZone>();

            zone.isDamaged = isDamaged;

            if (isDamaged)
            {
                inspectionManager.RegisterRealDamage(zone);
            }
        }

        inspectionManager.SetTotalDamage(totalPartsDamaged);
    }
}
