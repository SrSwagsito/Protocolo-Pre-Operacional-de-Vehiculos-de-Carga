using UnityEngine;
using TMPro;

public class TrashType : MonoBehaviour
{
    [SerializeField] private string trashName;
    [SerializeField] private TextMeshPro nombreTexto;

    public enum TrashTypes
    {
        organico,
        aprovechableGris,
        aprovechableBlanco,
        aprovechableAzul,
        peligroso,
        ordinario,
        sanitario,
        NoAprovechable
    }

    public TrashTypes tipoCorrecto;

    private void Start()
    {
        if (nombreTexto != null)
        {
            nombreTexto.text = trashName;
        }
    }

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