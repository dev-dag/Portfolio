using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dialog : BaseObject
{
    public bool IsActing { get; private set; } = false;

    [SerializeField] private TMP_Text dialogTMP;
    [SerializeField] private GameObject contentObject;

    private InputAction continueDialogAction;
    private List<string> dialogList;
    private int currentIndex = 0;

    protected override void Awake()
    {
        base.Awake();

        continueDialogAction = GameManager.Instance.globalInputActionAsset.FindActionMap("UI")?.FindAction("ContinueDialog");
        if (continueDialogAction == null)
        {
            Debug.LogError("Input Action ���� ����");
        }

        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // ��ǲ �׼ǿ� �̺�Ʈ�� ���������� ����
        continueDialogAction.started -= OnContinueDialog;
    }

    public void StartDialog(List<string> dialogList)
    {
        this.dialogList = dialogList;

        if (dialogList.Count == 0)
        {
            return;
        }

        this.gameObject.SetActive(true);

        IsActing = true;

        continueDialogAction.started += OnContinueDialog;

        SetDialog();
    }

    /// <summary>
    /// ���̾�α� �ؽ�Ʈ�� �����ϴ� �Լ�
    /// </summary>
    private void SetDialog()
    {
        dialogTMP.text = dialogList[currentIndex];
    }

    /// <summary>
    /// ContinueDialog ��ǲ�� ���� ��� �ݹ� �Լ�
    /// </summary>
    private void OnContinueDialog(InputAction.CallbackContext args)
    {
        currentIndex++;

        if (dialogList.Count > currentIndex)
        {
            SetDialog();
        }
        else
        {
            OnDialogEnd();
        }
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
        continueDialogAction.started -= OnContinueDialog;

        // ������Ƽ ���� ����
        IsActing = false;

        NotifyDialogEnd();
    }

    /// <summary>
    /// ���̾�αװ� �������� �̺�Ʈ�� �����ϴ� �Լ�
    /// </summary>
    private void NotifyDialogEnd()
    {

    }
}
