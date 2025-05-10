using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class NPC : MonoBehaviour, IInteractable
{
    public bool IsInit { get; protected set; } = false;

    [SerializeField] protected NPC_Data NPC_Data;

    protected OverheadUI overheadUI;
    private InputAction startDialogAction;

    private void Awake()
    {
        startDialogAction = GameManager.Instance.globalInputActionAsset.FindActionMap("UI")?.FindAction("Interact");
        if (startDialogAction == null)
        {
            EDebug.LogError("Input Action 참조 오류");
        }
    }

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        if (IsInit)
        {
            if (NPC_Data.overheadDialogID != -1)
            {
                CheckDistanceWithPlayer();
            }
        }
    }

    private void OnDestroy()
    {
        if (HasOverheadDialog())
        {
            overheadUI.Return();
        }
    }

    /// <summary>
    /// 다이얼로그 인스턴스 생성
    /// </summary>
    private void MakeOverheadUI()
    {
        overheadUI = GameManager.Instance.gameUI.OverheadUI_Pool.Burrow<OverheadUI>();
    }

    /// <summary>
    /// 다이얼로그 및 멤버필드 변수 초기화
    /// </summary>
    protected virtual void Init()
    {
        if (HasOverheadDialog())
        {
            MakeOverheadUI();

            RectTransform overheadUI_RTR = overheadUI.GetComponent<RectTransform>();
            overheadUI_RTR.anchoredPosition = (Vector2)transform.position + NPC_Data.overheadUI_Offset;

            overheadUI.Enable();
            SetOverheadDialog();
        }
        
        if (HasOverheadDialog())
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

                    if (overheadUI.IsActive(OverheadUI.Feature.Dialog) == false)
                    {
                        overheadUI.ActiveDialog(true);
                    }
                }
                else
                {
                    if (overheadUI.IsActive(OverheadUI.Feature.Dialog) == true)
                    {
                        overheadUI.ActiveDialog(false);
                    }
                }
            }

            await Awaitable.WaitForSecondsAsync(0.5f);
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
    protected virtual void StartDialog(Action callback = null)
    {
        if (GameManager.Instance.gameUI.Dialog.IsActing || HasDialog() == false)
        {
            return;
        }

        List<string> stringList = GameManager.Instance.ReferenceData.dialog[NPC_Data.dialogID].DialogTextList;

        GameManager.Instance.gameUI.Dialog.StartDialog(stringList, callback);
    }

    protected virtual void SetOverheadDialog()
    {
        overheadUI.SetDialogText(GameManager.Instance.ReferenceData.overheadDialog[NPC_Data.overheadDialogID].DialogText);
    }

    public bool IsInteractable()
    {
        return HasDialog();
    }

    public void SetInteractionGuide(bool isActive)
    {
        overheadUI.ActiveG_Key(isActive);
    }

    public void StartInteraction(Action interactionCallback)
    {
        overheadUI.gameObject.SetActive(false);

        StartDialog();

        GameManager.Instance.gameUI.Dialog.onDialogEndEvent += () =>
        {
            interactionCallback?.Invoke();
            overheadUI.gameObject.SetActive(true);
        };
    }

    public void CancelInteraction()
    {
        GameManager.Instance.gameUI.Dialog.StopDialog();
    }
}
