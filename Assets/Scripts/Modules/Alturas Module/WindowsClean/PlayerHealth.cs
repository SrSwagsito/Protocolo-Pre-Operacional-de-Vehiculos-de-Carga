using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : LifeSystem
{

    [SerializeField] LifeSystem lifeSys;
    // [SerializeField] Animator anim;
    [SerializeField] Button button;

    [Header("Config")]

    [SerializeField] float invulnerabiltyTime = 0.5f;
    [SerializeField] float cleanTime = 0.5f;

    bool invulnerable;
    WindowSpot currentWindow;

    public bool CanClean => currentWindow != null;

    public void TakeDamage(int damage)
    {
        if (invulnerable) return;

        lifeSys.ReceiveDamage(damage);

        StartCoroutine(Invulnerability());
    }
    
    public void CleanWindow()
    {
        if (currentWindow == null) return;

       // anim.SetTrigger("Clean");

        StartCoroutine(CleanRoutine());

    }

    public IEnumerator Invulnerability()
    {
        invulnerable = true;

        yield return new WaitForSeconds(invulnerabiltyTime);

        invulnerable = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        WindowSpot spot =
            other.GetComponent<WindowSpot>();

        if (spot != null)
        {

            currentWindow = spot;

            button.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        WindowSpot spot = other.GetComponent<WindowSpot>();

        if (spot == currentWindow)
        {
            currentWindow = null;
            button.gameObject.SetActive(false);
        }
    }

    public IEnumerator CleanRoutine()
    {
        yield return new WaitForSeconds(cleanTime);

        currentWindow.Clean();
    }

}
