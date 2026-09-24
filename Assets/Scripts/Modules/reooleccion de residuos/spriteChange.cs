using UnityEngine;

public class SpriteChange : MonoBehaviour
{
    [Header("Sprites de las canecas")]
    public Sprite spriteOrganico;
    public Sprite spriteNoAprovechable;
    public Sprite spritePeligroso;
    public Sprite spriteAprovechableAzul;
    public Sprite spriteAprovechableGris;
    public Sprite spriteOrdinario;

    public string currentType;

    public void SetColorSprite(GameObject target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sprite = spriteOrganico;
            target.tag = "organico";
            currentType = "organico";
        }
    }

    public void SetColorSpriteBlue(GameObject target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sprite = spriteAprovechableAzul;
            target.tag = "aprovechableAzul";
            currentType = "aprovechableAzul";
        }
    }

    public void SetColorSpriteGrey(GameObject target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sprite = spriteAprovechableGris;
            target.tag = "aprovechableGris";
            currentType = "aprovechableGris";
        }
    }

    public void SetColorSpriteGreen(GameObject target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sprite = spriteOrdinario;
            target.tag = "ordinario";
            currentType = "ordinario";
        }
    }
}