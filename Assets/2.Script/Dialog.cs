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
            Debug.LogError("Input Action 참조 오류");
        }

        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // 인풋 액션에 이벤트가 남아있으면 제거
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
    /// 다이얼로그 텍스트를 설정하는 함수
    /// </summary>
    private void SetDialog()
    {
        dialogTMP.text = dialogList[currentIndex];
    }

    /// <summary>
    /// ContinueDialog 인풋이 들어온 경우 콜백 함수
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
    /// 다이얼로그가 끝났을 때 호출되는 함수. 필드와 프로퍼티를 정리한다.
    /// </summary>
    private void OnDialogEnd()
    {
        currentIndex = 0;
        dialogList = null;

        this.gameObject.SetActive(false);

        // 인풋 액션에 달아놓은 이벤트 제거
        continueDialogAction.started -= OnContinueDialog;

        // 프로퍼티 상태 변경
        IsActing = false;

        NotifyDialogEnd();
    }

    /// <summary>
    /// 다이얼로그가 끝났음을 이벤트로 전달하는 함수
    /// </summary>
    private void NotifyDialogEnd()
    {

    }
}
