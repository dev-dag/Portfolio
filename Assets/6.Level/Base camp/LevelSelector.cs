using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private List<RectTransform> listArg;
    [SerializeField] private RectTransform cursorRtr;
    [SerializeField] private RectTransform cursorOriginTr;

    public event Action onExitEventHandler;

    private InputAction upAction;
    private InputAction downAction;
    private InputAction confirmAction;
    private InputAction cancelAction;

    private int index = 0;

    private bool isInit = false;

    private void Start()
    {
        InputActionAsset InputActionAsset = GameManager.Instance.globalInputActionAsset;

        InputActionMap UI_ActionMap = InputActionAsset.FindActionMap("UI");

        upAction = UI_ActionMap.FindAction("Up");
        downAction = UI_ActionMap.FindAction("Down");
        confirmAction = UI_ActionMap.FindAction("Confirm");
        cancelAction = UI_ActionMap.FindAction("Cancel");

        if (upAction == null
            || downAction == null
            || confirmAction == null
            || cancelAction == null)
        {
            EDebug.LogError("인풋 시스템 설정 오류");
        }

        upAction.performed += OnUp;
        downAction.performed += OnDown;
        confirmAction.performed += OnConfirm;
        cancelAction.performed += OnCancel;

        index = 0;

        isInit = true;

        Disable();
    }

    private void OnEnable()
    {
        if (isInit)
        {
            upAction.performed += OnUp;
            downAction.performed += OnDown;
            confirmAction.performed += OnConfirm;
            cancelAction.performed += OnCancel;

            index = 0;
        }

        cursorRtr.anchoredPosition = cursorOriginTr.anchoredPosition;
    }

    private void OnDisable()
    {
        upAction.performed -= OnUp;
        downAction.performed -= OnDown;
        confirmAction.performed -= OnConfirm;
        cancelAction.performed -= OnCancel;
    }

    public void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }

    private void OnUp(InputAction.CallbackContext context)
    {
        if (listArg.Count == 0)
        {
            return;
        }

        index--;

        RectTransform nextRTR = listArg[(listArg.Count + index) % listArg.Count];

        cursorRtr.anchoredPosition = nextRTR.anchoredPosition;
    }

    private void OnDown(InputAction.CallbackContext context)
    {
        if (listArg.Count == 0)
        {
            return;
        }

        index++;

        RectTransform nextRTR = listArg[(listArg.Count + index) % listArg.Count];

        cursorRtr.anchoredPosition = nextRTR.anchoredPosition;
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        upAction.performed -= OnUp;
        downAction.performed -= OnDown;
        confirmAction.performed -= OnConfirm;

        onExitEventHandler?.Invoke();

        GameManager.Instance.ChangeMap(1);
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        Disable();

        onExitEventHandler?.Invoke();
    }
}
