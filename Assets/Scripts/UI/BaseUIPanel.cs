using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class BaseUIPanel : MonoBehaviour, IUIPanel
{
    [Header("Panel Info")]
    public string PanelName { get => _panelName; set => _panelName = value; }

    public bool IsActive => gameObject.activeSelf;
    [SerializeField] string _panelName;
    [SerializeField] bool resetPanel = false;

    Animator animator;

    int hideHash = Animator.StringToHash("Hide");
    int showHash = Animator.StringToHash("Show");
    int idleHash = Animator.StringToHash("Idle");

    public virtual void Start()
    {
    }
    public virtual void Awake()
    {
        TryGetComponent(out animator);
    }
    public virtual void Show()
    {
        if (resetPanel)
        {
            OnReset();
        }
    }
    public virtual void OnReset()
    {
    }
    public virtual void Hide()
    {
    }
    public IEnumerator HideCoroutine()
    {
        if (animator == null)
            yield break;

        bool animation = animator.HasState(0, hideHash);
        if (animation)
        {
            animator?.Play(hideHash);
            yield return WaitForAnimation("Hide");
        }
        else
            yield break;
    }
    public IEnumerator ShowCoroutine(bool instant)
    {
        if (animator == null)
            yield break;

        bool animation = animator.HasState(0, instant ? idleHash : showHash);
        if (animation)
        {
            animator?.Play(instant ? idleHash : showHash);
            yield return WaitForAnimation(instant ? "Idle" : "Show");
        }
        else
            yield break;
    }
    protected IEnumerator WaitForAnimation(string stateName)
    {
        var clipInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (clipInfo.IsName(stateName) && clipInfo.normalizedTime < 1f)
        {
            yield return null;
            clipInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
    }
}
