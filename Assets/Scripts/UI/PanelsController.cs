using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelsController : BaseUIPanel
{
    [Header("Menus Info")]
    public List<BaseUIPanel> Panels { get => _panels; set => _panels = value; }
    [SerializeField] List<BaseUIPanel> _panels;
    [SerializeField] BaseUIPanel currentPanel;
    BaseUIPanel lastPanel;
    [SerializeField] BaseUIPanel initialPanel;
    bool transition = false;

    public override void Awake()
    {
        base.Awake();
    }
    public override void Start()
    {
        ShowInitialPanel();
    }
    public override void OnReset()
    {
        ShowInitialPanel();
    }
    public void ShowInitialPanel()
    {
        if (initialPanel != null)
        {
            ChangePanel(initialPanel.PanelName, null, true);
        }
    }
    public void ChangePanel(string panelName, Action finishAction = null, bool instant = false)
    {
        StartCoroutine(ChangePanelCoroutine(panelName, null, instant));
    }
    public void ChangePanel(string panelName, Action finishAction = null)
    {
        StartCoroutine(ChangePanelCoroutine(panelName, finishAction));
    }
    public void ChangePanel(string panelName)
    {
        StartCoroutine(ChangePanelCoroutine(panelName));
    }
    public IEnumerator ChangePanelCoroutine(string panelName, Action finishAction = null, bool instant = false)
    {
        BaseUIPanel panel = Panels.Find(x => x.PanelName == panelName);

        if (transition || !panel || panel == currentPanel)
            yield break;

        transition = true;
        yield return CloseCurrentPanel();
        currentPanel = panel;
        currentPanel.gameObject.SetActive(true);
        currentPanel?.Show();
        yield return currentPanel?.ShowCoroutine(instant);
        finishAction?.Invoke();
        lastPanel?.gameObject.SetActive(false);
        transition = false;
    }
    public IEnumerator CloseCurrentPanel()
    {
        yield return currentPanel?.HideCoroutine();
        lastPanel = currentPanel;
        currentPanel = null;
    }
}
