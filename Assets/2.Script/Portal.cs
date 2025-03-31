using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Portal : BaseObject, IInteractable
{
    [SerializeField] private LevelSelector levelSelector;

    private OverheadUI overheadUI;
    private Action callback;

    protected override void Start()
    {
        base.Start();

        overheadUI = GameManager.Instance.uiManager.overheadUI_Pool.Burrow<OverheadUI>();

        RectTransform overheadUI_RTR = overheadUI.GetComponent<RectTransform>();
        overheadUI_RTR.anchoredPosition = (Vector2)transform.position + Vector2.up * 1.5f;

        overheadUI.Enable();

        levelSelector.onExitEventHandler += OnInteractionExit;
    }

    private void OnDestroy()
    {
        overheadUI.Return();
    }

    public void CancelInteraction()
    {
        levelSelector.Disable();
    }

    public void StartInteraction(Action interactionCallback)
    {
        levelSelector.Enable();
        callback = interactionCallback;
    }

    public bool IsInteractable()
    {
        return true;
    }

    public void SetInteractionGuide(bool isActive)
    {
        overheadUI.Active(OverheadUI.Feature.GKeyIcon, isActive);
    }

    private void OnInteractionExit()
    {
        callback?.Invoke();
    }
}
