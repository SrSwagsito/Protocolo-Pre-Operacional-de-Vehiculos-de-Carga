using UnityEngine;

public class SpriteChange : MonoBehaviour
{
    public string currentType;
    public void SetColorSprite(GameObject target)
    {
        ColorUtility.TryParseHtmlString("#F5BF7E", out Color color);
        target.GetComponent<SpriteRenderer>().color = color;
        target.tag = "organico";

    }
    public void SetColorSpriteBlack(GameObject target)
    {
        target.GetComponent<SpriteRenderer>().color = Color.black;
        target.tag = "noAprovechable";

    }
    public void SetColorSpriteRed(GameObject target)
    {
        target.GetComponent<SpriteRenderer>().color = Color.red;
        target.tag = "peligroso";

    }
    public void SetColorSpriteBlue(GameObject target)
    {
        target.GetComponent<SpriteRenderer>().color = Color.blue;
        target.tag = "aprovechableAzul";

    }
    public void SetColorSpriteGrey(GameObject target)
    {
        target.GetComponent<SpriteRenderer>().color = Color.grey;
        target.tag = "aprovechableGris";

    }
    public void SetColorSpriteGreen(GameObject target)
    {
        target.GetComponent<SpriteRenderer>().color = Color.green;
        target.tag = "ordinario";

    }
}