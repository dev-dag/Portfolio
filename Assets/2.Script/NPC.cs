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
            Debug.LogError("Input Action ���� ����");
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
    /// ���̾�α� �ν��Ͻ� ����
    /// </summary>
    private void MakeOverheadUI()
    {
        overheadUI = GameManager.Instance.uiManager.overheadUI_Pool.Burrow<OverheadUI>();
    }

    /// <summary>
    /// ���̾�α� �� ����ʵ� ���� �ʱ�ȭ
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
    /// �÷��̾�� �Ÿ��� üũ�ؼ� ���̾�α׸� ����ϰų� Ű���� �Է� ����� UI�� ����ϴ� �Լ�
    /// </summary>
    private async Awaitable CheckDistanceWithPlayer()
    {
        while (gameObject.activeSelf)
        {
            Vector2 distance = transform.position - Player.Current.transform.position;

            if (HasOverheadDialog())
            {
                // ������� ���̾�α� �߻� ���� üũ
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
                // G Key UI �߻� ���� üũ
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
    /// G Key �������� �����ϰ� Ű���� �Է� �̺�Ʈ�� ���/�����ϴ� �Լ�
    /// </summary>
    /// <param name="isActive"></param>
    private void SetActiveGKeyIcon(bool isActive)
    {
        if (isG_KeyIconActive != isActive) // ���� ���� ��쿡 �̺�Ʈ ���/����
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
    /// StartDialog Input�� �߻����� �� ȣ��Ǵ� �Լ�. �÷��̾���� �Ÿ��� ����� ���͸� �� �� notify
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
    /// ��� ������ ���̾�αװ� �ִ� ��� True ��ȯ
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
    /// ��� ������ ������� ���̾�αװ� �ִ� ��� True ��ȯ
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
    /// ���̾�α� �ν��Ͻ��� ����� ��ȭ ����
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
