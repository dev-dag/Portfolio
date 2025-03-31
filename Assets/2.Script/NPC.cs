using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class NPC : BaseObject, IInteractable
{
    public bool IsInit { get; protected set; } = false;

    [SerializeField] protected NPC_Data NPC_Data;

    protected OverheadUI overheadUI;
    private InputAction startDialogAction;

    protected override void Awake()
    {
        base.Awake();

        startDialogAction = GameManager.Instance.globalInputActionAsset.FindActionMap("UI")?.FindAction("Interact");
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
            if (NPC_Data.overheadDialogID != -1)
            {
                CheckDistanceWithPlayer();
            }
        }
    }

    private void OnDestroy()
    {
        overheadUI.Return();
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

        overheadUI.Enable();
        SetOverheadDialog();

        if (HasOverheadDialog())
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
    protected virtual void StartDialog()
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
        overheadUI.SetDialogText(GameManager.Instance.data.overheadDialog[NPC_Data.overheadDialogID].DialogText);
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

        GameManager.Instance.uiManager.dialog.onDialogEndEvent += () =>
        {
            interactionCallback?.Invoke();
            overheadUI.gameObject.SetActive(true);
        };
    }

    public void CancelInteraction()
    {
        GameManager.Instance.uiManager.dialog.StopDialog();
    }
}
