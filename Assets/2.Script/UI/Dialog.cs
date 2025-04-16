using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dialog : BaseObject
{
    public bool IsActing { get; private set; } = false;
    public event Action onDialogEndEvent;

    [SerializeField] private TMP_Text dialogTMP;
    [SerializeField] private GameObject contentObject;

    private InputAction continueDialogAction;
    private List<string> dialogList;
    private int currentIndex = 0;

    private Awaitable typingAwaiter = null;

    protected override void Awake()
    {
        base.Awake();

        continueDialogAction = GameManager.Instance.globalInputActionAsset.FindActionMap("UI")?.FindAction("ContinueDialog");
        if (continueDialogAction == null)
        {
            EDebug.LogError("Input Action 참조 오류");
        }

        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // 인풋 액션에 이벤트가 남아있으면 제거
        continueDialogAction.performed -= OnContinueDialog;
    }

    public void StartDialog(List<string> dialogList, Action callback = null)
    {
        this.dialogList = dialogList;

        onDialogEndEvent += callback;

        if (dialogList.Count == 0)
        {
            return;
        }

        this.gameObject.SetActive(true);

        IsActing = true;

        continueDialogAction.performed += OnContinueDialog;

        SetDialog();
    }

    /// <summary>
    /// 다이얼로그를 정지하는 함수
    /// </summary>
    public void StopDialog()
    {
        OnDialogEnd();
    }

    /// <summary>
    /// 다이얼로그 텍스트를 설정하는 함수
    /// </summary>
    private void SetDialog()
    {
        if (typingAwaiter != null) // 타이핑중인 경우
        {
            typingAwaiter.Cancel(); // 타이핑 로직을 취소하고 즉시 전체 텍스트를 출력함.
            typingAwaiter = null;

            dialogTMP.text = dialogList[currentIndex];
        }
        else // 다음 다이얼로그로 진행 or 종료
        {
            if (dialogList.Count > currentIndex)
            {
                typingAwaiter = TypingText();
            }
            else
            {
                OnDialogEnd();
            }
        }
    }

    /// <summary>
    /// ContinueDialog 인풋이 들어온 경우 콜백 함수
    /// </summary>
    private void OnContinueDialog(InputAction.CallbackContext args)
    {
        if (typingAwaiter == null)
        {
            currentIndex++; 
        }

        SetDialog();
    }

    /// <summary>
    /// 다이얼로그가 끝났을 때 호출되는 함수. 필드와 프로퍼티를 정리한다.
    /// </summary>
    private void OnDialogEnd()
    {
        currentIndex = 0;
        dialogList = null;

        this.gameObject.SetActive(false);

        // 인풋 액션에 달아놓은 이벤트 제거
        continueDialogAction.performed -= OnContinueDialog;

        // 프로퍼티 상태 변경
        IsActing = false;

        onDialogEndEvent?.Invoke();
        onDialogEndEvent = null;

        if (typingAwaiter != null)
        {
            typingAwaiter.Cancel();
            typingAwaiter = null;
        }
    }

    private async Awaitable TypingText()
    {
        dialogTMP.text = string.Empty;

        string typingBuffer = string.Empty;
        int typingIndex = 0;

        while (dialogTMP.text.Length < dialogList[currentIndex].Length)
        {
            typingBuffer = $"{typingBuffer}{dialogList[currentIndex][typingIndex]}";
            dialogTMP.text = typingBuffer;

            typingIndex++;

            await Awaitable.WaitForSecondsAsync(0.01f);
        }

        typingAwaiter = null;

        return;
    }
}
