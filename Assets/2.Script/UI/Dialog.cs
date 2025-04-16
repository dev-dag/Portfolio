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
            EDebug.LogError("Input Action ���� ����");
        }

        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // ��ǲ �׼ǿ� �̺�Ʈ�� ���������� ����
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
    /// ���̾�α׸� �����ϴ� �Լ�
    /// </summary>
    public void StopDialog()
    {
        OnDialogEnd();
    }

    /// <summary>
    /// ���̾�α� �ؽ�Ʈ�� �����ϴ� �Լ�
    /// </summary>
    private void SetDialog()
    {
        if (typingAwaiter != null) // Ÿ�������� ���
        {
            typingAwaiter.Cancel(); // Ÿ���� ������ ����ϰ� ��� ��ü �ؽ�Ʈ�� �����.
            typingAwaiter = null;

            dialogTMP.text = dialogList[currentIndex];
        }
        else // ���� ���̾�α׷� ���� or ����
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
    /// ContinueDialog ��ǲ�� ���� ��� �ݹ� �Լ�
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
    /// ���̾�αװ� ������ �� ȣ��Ǵ� �Լ�. �ʵ�� ������Ƽ�� �����Ѵ�.
    /// </summary>
    private void OnDialogEnd()
    {
        currentIndex = 0;
        dialogList = null;

        this.gameObject.SetActive(false);

        // ��ǲ �׼ǿ� �޾Ƴ��� �̺�Ʈ ����
        continueDialogAction.performed -= OnContinueDialog;

        // ������Ƽ ���� ����
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
