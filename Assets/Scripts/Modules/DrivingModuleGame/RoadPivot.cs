using UnityEngine;

/// <summary>
/// Pivote individual que define un punto del camino.
/// Tiene un offset en Y que determina la curvatura.
/// </summary>
public class RoadPivot : MonoBehaviour
{
    [Tooltip("Desplazamiento vertical del pivote (define la curva)")]
    public float offset = 0f;

    private void OnDrawGizmos()
    {
        // Dibujar esfera en la posición del pivote
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, offset, 0), 0.3f);
    }
}
