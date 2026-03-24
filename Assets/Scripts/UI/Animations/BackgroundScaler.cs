using UnityEngine;

[ExecuteInEditMode]
public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        ScaleBackground();
    }

    // Usamos Update solo si quieres que se ajuste al cambiar la resolución en el editor
    void Update()
    {
        ScaleBackground();
    }

    void ScaleBackground()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Resetear escala para calcular desde el tamaño base
        transform.localScale = Vector3.one;

        var width = sr.sprite.bounds.size.x;
        var height = sr.sprite.bounds.size.y;

        var worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        var worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        // Aplicar escala para cubrir todo el ancho y alto
        transform.localScale = new Vector3(worldScreenWidth / width, worldScreenHeight / height, 1);
    }
}