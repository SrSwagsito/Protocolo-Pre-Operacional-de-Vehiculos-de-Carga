using System.Collections;
using UnityEngine;

public class BinType : MonoBehaviour
{
    public TrashType.TrashTypes tipoCaneca;

    [Header("Sprite temporal")]
    public Sprite spriteTemporal;
    public float tiempoCambio = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Sprite spriteOriginal;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteOriginal = spriteRenderer.sprite;
    }

    public void CambiarSpriteTemporal()
    {
        StartCoroutine(CambioTemporal());
    }

    private IEnumerator CambioTemporal()
    {
        spriteRenderer.sprite = spriteTemporal;

        yield return new WaitForSeconds(tiempoCambio);

        spriteRenderer.sprite = spriteOriginal;
    }
}
