using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPC : BaseObject
{
    public bool IsInit { get; protected set; } = false;

    [SerializeField] protected NPC_Data NPC_Data;

    protected OverheadUI overheadUI;
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
            if (NPC_Data.dialogID != -1 && NPC_Data.overheadDialogID != -1)
            {
                CheckDistanceWithPlayer();
            }
        }
    }

    private void OnDisable()
    {
        startDialogAction.started -= OnStartDialog;
    }

    /// <summary>
    /// 다이얼로그 인스턴스 생성
    /// </summary>
    private void MakeOverheadUI()
    {
        overheadUI = GameManager.Instance.uiManager.overheadUI_Pool.Burrow<OverheadUI>();
    }

    /// <summary>
    /// 다이얼로그 및 멤버필드 변수 초기화
    /// </summary>
    protected virtual void Init()
    {
        MakeOverheadUI();

        RectTransform overheadUI_RTR = overheadUI.GetComponent<RectTransform>();
        overheadUI_RTR.anchoredPosition = (Vector2)transform.position + NPC_Data.overheadUI_Offset;

        overheadUI.Active(OverheadUI.Feature.ALL, false);
        SetOverheadDialog();

        isG_KeyIconActive = false;

        if (HasDialog() || HasOverheadDialog())
        {
            CheckDistanceWithPlayer();
        }
        
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

            if (HasOverheadDialog())
            {
                // 오버헤드 다이얼로그 발생 조건 체크
                if (Math.Abs(distance.x) < NPC_Data.overheadUI_Distance && Math.Abs(distance.y) < NPC_Data.overheadUI_Distance)
                {
                    SetOverheadDialog();
                    overheadUI.Active(OverheadUI.Feature.Dialog, true);
                }
                else
                {
                    overheadUI.Active(OverheadUI.Feature.Dialog, false);
                }
            }

            if (HasDialog())
            {
                // G Key UI 발생 조건 체크
                if (GameManager.Instance.uiManager.dialog.IsActing)
                {
                    SetActiveGKeyIcon(false);
                }
                else if (Math.Abs(distance.x) < NPC_Data.gKeyIconDistance && Math.Abs(distance.y) < NPC_Data.gKeyIconDistance)
                {
                    SetActiveGKeyIcon(true);
                }
                else
                {
                    SetActiveGKeyIcon(false);
                }
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
        
        if (Math.Abs(distance.x) < NPC_Data.gKeyIconDistance && Math.Abs(distance.y) < NPC_Data.gKeyIconDistance)
        {
            SetDialog();
        }
    }

    /// <summary>
    /// 출력 가능한 다이얼로그가 있는 경우 True 반환
    /// </summary>
    protected virtual bool HasDialog()
    {
        if (NPC_Data.dialogID != -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 출력 가능한 오버헤드 다이얼로그가 있는 경우 True 반환
    /// </summary>
    protected virtual bool HasOverheadDialog()
    {
        if (NPC_Data.overheadDialogID != -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 다이얼로그 인스턴스를 사용해 대화 시작
    /// </summary>
    protected virtual void SetDialog()
    {
        if (GameManager.Instance.uiManager.dialog.IsActing || HasDialog() == false)
        {
            return;
        }

        List<string> stringList = GameManager.Instance.data.dialog[NPC_Data.dialogID].DialogTextList;

        GameManager.Instance.uiManager.dialog.StartDialog(stringList);
    }

    protected virtual void SetOverheadDialog()
    {
        overheadUI.SetText(GameManager.Instance.data.overheadDialog[NPC_Data.overheadDialogID].DialogText);
    }
}
