using System.Collections;
using UnityEngine;

public class BinType : MonoBehaviour
{
    public TrashType.TrashTypes tipoCaneca;

    [Header("Sprite temporal")]
    public Sprite spriteTemporal;
    public Color colorTemporal = Color.white; // Selector de color
    public float tiempoCambio = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Sprite spriteOriginal;
    private Color colorOriginal;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteOriginal = spriteRenderer.sprite;
        colorOriginal = spriteRenderer.color;
    }

    public void CambiarSpriteTemporal()
    {
        StartCoroutine(CambioTemporal());
    }

    private IEnumerator CambioTemporal()
    {
        spriteRenderer.sprite = spriteTemporal;
        spriteRenderer.color = colorTemporal;

        yield return new WaitForSeconds(tiempoCambio);

        spriteRenderer.sprite = spriteOriginal;
        spriteRenderer.color = colorOriginal;
    }
}