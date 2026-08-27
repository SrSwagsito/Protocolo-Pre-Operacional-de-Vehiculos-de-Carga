using System.Collections.Generic;
using UnityEngine;

public class TrashType : MonoBehaviour
{ 

    public enum TrashTypes
    {
        organico,
        aprovechable,
        peligroso,
        ordinario,
        sanitario,
        NoAprovechable
    }
    public TrashTypes tipoCorrecto;
    private void OnTriggerEnter2D(Collider2D other)

    {

        BinType bin = other.GetComponent<BinType>();


        if (bin != null)
        {
            bin.CambiarSpriteTemporal();

            if (bin.tipoCaneca == tipoCorrecto)
            {
                TrashInspectionManager.Instance.RegisterCorrectTrash(this);
            }
            else
            {
                TrashInspectionManager.Instance.RegisterWrongTrash(this);
            }

            TrashSpawner.Instance.TrashDestroyed();
        }

    }
 
}
