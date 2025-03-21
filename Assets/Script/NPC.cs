using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPC : BaseObject
{
    public bool IsInit { get; private set; } = false;

    public int dialogID = -1;
    public int overheadDialogID = -1;

    [Space(20f)]
    [SerializeField] private Vector2 overheadUI_Offset;
    [SerializeField] private float overheadUI_Distance;
    [SerializeField] private float gKeyIconDistance;

    private OverheadUI overheadUI;
    private InputAction startDialogAction;

    private bool isG_KeyIconActive = false;

    protected override void Awake()
    {
        base.Awake();

        startDialogAction = GameManager.Instance.globalInputActionAsset.FindActionMap("UI")?.FindAction("StartDialog");
        if (startDialogAction == null)
        {
            Debug.LogError("Input Action 참조 오류");
        }
    }

    protected override void Start()
    {
        base.Start();

        Init();
    }

    private void OnEnable()
    {
        if (IsInit)
        {
            CheckDistanceWithPlayer();
        }
    }

    private void OnDisable()
    {
        startDialogAction.started -= OnStartDialog;
    }

    private void MakeOverheadUI()
    {
        overheadUI = GameManager.Instance.uiManager.overheadUI_Pool.Burrow<OverheadUI>();
    }

    private void Init()
    {
        MakeOverheadUI();

        RectTransform overheadUI_RTR = overheadUI.GetComponent<RectTransform>();
        overheadUI_RTR.anchoredPosition = (Vector2)transform.position + overheadUI_Offset;

        overheadUI.SetText(GameManager.Instance.data.overheadDialog[0].DialogText);
        overheadUI.Active(OverheadUI.Feature.ALL, false);

        isG_KeyIconActive = false;

        CheckDistanceWithPlayer();
        
        IsInit = true;
    }

    /// <summary>
    /// 플레이어와 거리를 체크해서 다이얼로그를 출력하거나 키보드 입력 도우미 UI를 출력하는 함수
    /// </summary>
    private async Awaitable CheckDistanceWithPlayer()
    {
        while (gameObject.activeSelf)
        {
            Vector2 distance = transform.position - Player.Current.transform.position;

            // 오버헤드 다이얼로그 발생 조건 체크
            if (Math.Abs(distance.x) < overheadUI_Distance && Math.Abs(distance.y) < overheadUI_Distance)
            {
                overheadUI.Active(OverheadUI.Feature.Dialog, true);
            }
            else
            {
                overheadUI.Active(OverheadUI.Feature.Dialog, false);
            }

            // G Key UI 발생 조건 체크
            if (GameManager.Instance.uiManager.dialog.IsActing)
            {
                SetActiveGKeyIcon(false);
            }
            else if (Math.Abs(distance.x) < gKeyIconDistance && Math.Abs(distance.y) < gKeyIconDistance)
            {
                SetActiveGKeyIcon(true);
            }
            else
            {
                SetActiveGKeyIcon(false);
            }

            await Awaitable.WaitForSecondsAsync(0.1f);
        }
    }

    /// <summary>
    /// G Key 아이콘을 노출하고 키보드 입력 이벤트를 등록/해지하는 함수
    /// </summary>
    /// <param name="isActive"></param>
    private void SetActiveGKeyIcon(bool isActive)
    {
        if (isG_KeyIconActive != isActive) // 값이 변한 경우에 이벤트 등록/해지
        {
            if (isActive == true)
            {
                startDialogAction.started += OnStartDialog;
            }
            else
            {
                startDialogAction.started -= OnStartDialog;
            }

            isG_KeyIconActive = isActive;
        }

        overheadUI.Active(OverheadUI.Feature.GKeyIcon, isActive);
    }

    /// <summary>
    /// StartDialog Input이 발생했을 때 호출되는 함수. 플레이어와의 거리를 사용해 필터링 한 후 notify
    /// </summary>
    private void OnStartDialog(InputAction.CallbackContext args)
    {
        Vector2 distance = transform.position - Player.Current.transform.position;

        if (Math.Abs(distance.x) < gKeyIconDistance && Math.Abs(distance.y) < gKeyIconDistance)
        {
            StartDialog();
        }
    }

    private void StartDialog()
    {
        if (GameManager.Instance.uiManager.dialog.IsActing)
        {
            return;
        }

        List<string> stringList = GameManager.Instance.data.dialog[dialogID].Select((dialog) => dialog.DialogText).ToList();

        GameManager.Instance.uiManager.dialog.StartDialog(stringList);
    }
}
