using System.Collections.Generic;
using UnityEngine;

public class TrashType : MonoBehaviour
{
    public enum TrashTypes
    {
        organico,
        aprovechable,
        peligroso
    }
    public TrashTypes tipoCorrecto;
    private void OnTriggerEnter2D(Collider2D other)

    {

        BinType bin = other.GetComponent<BinType>();

 

            if (bin != null)

            {

            if (bin.tipoCaneca == tipoCorrecto)

            {

            Debug.Log("¡Correcto!");

            Destroy(gameObject);

            }

            else
    
            {
    
            Debug.Log("No correcto amiguirs");
    
            Destroy(gameObject);
    
            }
            TrashSpawner.Instance.TrashDestroyed();
            
          

        }

}
 
}
