using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ItemSlot;

public class Inventory : BaseObject
{
    public Dictionary<int, ItemContainer> Items { get; private set; } = new Dictionary<int, ItemContainer>();

    [Space(20f)]
    [SerializeField] private List<ItemSlot> bagItemSlots;
    [SerializeField] private ItemSlot weaponSlot;
    [SerializeField] private List<ExclusiveItemSlot> quickItemSlots;

    [Space(20f)]
    [SerializeField] private Image holdingItemImage;

    private GraphicRaycaster raycaster;
    private bool onHolding = false;

    /// <summary>
    /// �κ��丮 ��� �� ȣ��Ǿ�� �ϴ� �Լ�
    /// </summary>
    public void Init()
    {
        raycaster = GetComponentInParent<GraphicRaycaster>();

        // ���� ������ ���� �ʱ�ȭ
        foreach (ItemSlot bagItemSlot in bagItemSlots)
        {
            bagItemSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        weaponSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering); // ���� ���� �ʱ�ȭ

        // ������ �ʱ�ȭ
        foreach (ItemSlot quickItemSlot in quickItemSlots)
        {
            quickItemSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        // �κ��丮 ����Ű ����
        InputActionMap UI_ActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("UI");
        UI_ActionMap.FindAction("Cancel").performed += (arg) => Disable();
        UI_ActionMap.FindAction("Inventory").performed += (arg) =>
        {
            if (this.gameObject.activeInHierarchy == false)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        };
    }

    protected override void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        InputActionMap playerActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("Player");
        playerActionMap.FindAction("UseSkill_0").Disable();
        playerActionMap.FindAction("UseSkill_1").Disable();
        playerActionMap.FindAction("UseSkill_2").Disable();
    }

    private void OnDisable()
    {
        InputActionMap playerActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("Player");
        playerActionMap.FindAction("UseSkill_0").Enable();
        playerActionMap.FindAction("UseSkill_1").Enable();
        playerActionMap.FindAction("UseSkill_2").Enable();

        GameManager.Instance.uiManager.itemInfo.Disable();
    }   

    [ContextMenu("Add Potion")]
    public void AddPotion()
    {
        AddItem(3, 1);
    }

    /// <summary>
    /// �������� �߰��ϴ� �Լ�
    /// </summary>
    public bool AddItem(int itemID, int amount)
    {
        if (Items.TryGetValue(itemID, out ItemContainer container))
        {
            container.Amount += amount;

            return true;
        }
        else
        {
            GameManager.Instance.LoadItemInfo<ItemInfoData>(itemID); // ������ ���� �ε�

            if (GetEmptyBagItemSlot(out ItemSlot emptyBagSlot))
            {
                ItemContainer newContainer = ItemContainer.CreateItemContainer(itemID, amount);
                newContainer.OnValueChanged += CheckItemValid;

                Items.Add(itemID, newContainer);
                emptyBagSlot.Set(newContainer);

                for (int index = 0; index < quickItemSlots.Count; index++)
                {
                    ExclusiveItemSlot quickItemSlot = quickItemSlots[index];

                    if (quickItemSlot.ItemID != null && quickItemSlot.ItemID == itemID)
                    {
                        if (quickItemSlot.ItemAmount == 0) // ������ �������� �ִ� ��� �����̳� ���۷����� �ٽ� ��������� ��.
                        {
                            SetQuickSlot(index, newContainer);
                            break;
                        }
                    }
                }

                return true;
            }
            else
            {
                return false; // ���� ���� ����
            }
        }
    }

    /// <summary>
    /// �κ��丮 Ȱ��ȭ
    /// </summary>
    public void Enable()
    {
        if (this.gameObject.activeInHierarchy == false)
        {
            this.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// �κ��丮 ��Ȱ��ȭ
    /// </summary>
    public void Disable()
    {
        if (this.gameObject.activeInHierarchy == true)
        {
            this.gameObject.SetActive(false);
        }
    }

    private void CheckItemValid(ItemContainer container)
    {
        if (container.Amount <= 0)
        {
            container.OnValueChanged -= CheckItemValid;
            Items.Remove(container.Item.ID);
        }
    }

    /// <summary>
    /// ���� �巡�� �� ó��
    /// </summary>
    private void OnSlotDragging(ItemSlot trigger, Vector2 position, ItemSlot.InputStatus status)
    {
        if (trigger.ItemID == null
             || Items.ContainsKey(trigger.ItemID.Value) == false)
        {
            return;
        }

        switch (status)
        {
            case ItemSlot.InputStatus.OnProcessing: // �巡�� ���� ���
            {
                trigger.SetAlpha(0.5f); // trigger ���� 0.5f

                // Ȧ�� �̹��� ó��
                if (holdingItemImage.gameObject.activeInHierarchy == false)
                {
                    holdingItemImage.gameObject.SetActive(true);
                }

                holdingItemImage.sprite = trigger.ItemIconSprite; // holding image Ȱ��ȭ �� �����۰� ���� �̹����� ����

                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, position, null, out Vector2 pos);

                (holdingItemImage.transform as RectTransform).anchoredPosition = pos; // holding image position ����

                onHolding = true;

                break;
            }
            case ItemSlot.InputStatus.Ended: // �巡�� ���� �� ���
            {
                // ���� ĳ������ ���� �ش� Ŀ�� ��ġ�� ������ ���� B �˻�
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                List<RaycastResult> resultList = new List<RaycastResult>();

                raycaster.Raycast(pointerEventData, resultList);

                if (resultList.Count >= 1)
                {
                    ItemSlot dropSlot = resultList[0].gameObject.GetComponent<ItemSlot>();
                    // B ������ Bag�� ������ �������� üũ

                    if (dropSlot != null)
                    {
                        if (dropSlot == weaponSlot // Weapon Slot ���� üũ
                            && Items[trigger.ItemID.Value].Item.TypeEnum == Database_Table.Item.ItemType.Weapon) // trigger�� ���� Ÿ���� ��� ����.
                        {
                            SwapSlotItem(trigger, dropSlot);
                        }
                        else if (dropSlot is ExclusiveItemSlot // Quick Slot ���� üũ
                            && Items[trigger.ItemID.Value].Item.TypeEnum == Database_Table.Item.ItemType.Potion) // trigger�� ���� Ÿ���� ���
                        {
                            if (trigger is ExclusiveItemSlot) // trigger�� �������̺� ������ �� ����
                            {
                                ItemContainer dropContainer = null;
                                if (dropSlot.ItemID != null)
                                {
                                    dropContainer = Items[dropSlot.ItemID.Value];
                                }

                                ItemContainer triggerContainer = null;
                                if (trigger.ItemID != null)
                                {
                                    triggerContainer = Items[trigger.ItemID.Value];
                                }

                                SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), dropContainer);
                                SetQuickSlot(quickItemSlots.IndexOf(dropSlot as ExclusiveItemSlot), triggerContainer);
                            }
                            else
                            {
                                for (int index = 0; index < quickItemSlots.Count; index++) // �ش� �������� �� ���Կ� �ִ��� �ߺ� üũ
                                {
                                    ExclusiveItemSlot quickItemSlot = quickItemSlots[index];

                                    if (dropSlot != quickItemSlot // �ű���� ������ ��ġ ��󿡼� ����
                                        && quickItemSlot.ItemID != null && quickItemSlot.ItemID.Value == trigger.ItemID.Value)
                                    {
                                        SetQuickSlot(index, null);
                                        break;
                                    }
                                }

                                SetQuickSlot(quickItemSlots.IndexOf(dropSlot as ExclusiveItemSlot), Items[trigger.ItemID.Value]); // ���� ���� ������ ����
                            }
                        }
                        else
                        {
                            SwapSlotItem(trigger, dropSlot);
                        }
                    }
                }
                else if (trigger is ExclusiveItemSlot) // �������� �巡���ؼ� ����� ������ �����ĸ� ���ϴ� ���
                {
                    SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), null); // ������ ����
                }

                Player.Current.EquipWeapon(weaponSlot.ItemID); // �������� ���Ⱑ ����� ��� ó��

                trigger.SetAlpha(1f);

                holdingItemImage.gameObject.SetActive(false);
                onHolding = false;

                break;
            }
            default:
                break;
        }
    }

    /// <summary>
    /// ���� Ŭ�� �� ó��
    /// </summary>
    private void OnSlotClicked(ItemSlot trigger, PointerEventData.InputButton button)
    {
        if (trigger.ItemID == null)
        {
            return;
        }
        else if (trigger is ExclusiveItemSlot
                    && trigger.ItemAmount.Value <= 0) // �� ������ ��� ������ 0���� �� ��ȣ�ۿ� �Ұ����� ���°� ����.
        {
            SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), null); // �ش� ���¿��� ��Ŭ�� �� ���� ĳ�̱��� ����

            return;
        }

        ItemContainer container = Items[trigger.ItemID.Value];

        if (button == PointerEventData.InputButton.Right) // ��Ŭ���� ���
        {
            if (trigger == weaponSlot) // trigger�� ���� ������ ���
            {
                if (GetEmptyBagItemSlot(out ItemSlot emptyBagSlot))
                {
                    SwapSlotItem(trigger, emptyBagSlot);
                }
            }
            else if (trigger is ExclusiveItemSlot) // trigger�� �� ������ ���)
            {
                SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), null);
            }
            else if (container.Item.TypeEnum == Database_Table.Item.ItemType.Weapon) // trigger�� ����ǰ �����̰� ���� Ÿ�� �������� ���
            {
                SwapSlotItem(trigger, weaponSlot);
            }
            else if (container.Item.TypeEnum == Database_Table.Item.ItemType.Potion) // trigger�� ����ǰ �����̰� ���� Ÿ�� �������� ���
            {
                (container as Potion).Drink();
            }

            Player.Current.EquipWeapon(weaponSlot.ItemID); // �������� ���Ⱑ ����� ��� ó��
        }
    }

    /// <summary>
    /// ���� ȣ���� �� ó��
    /// </summary>
    private void OnSlotHovering(ItemSlot trigger, Vector2 position, InputStatus status)
    {
        if (onHolding) // �巡�� ���̸� �۵� ����.
        {
            return;
        }
        else if (trigger.ItemID == null) // ���Կ� �������� ��� ��ȯ.
        {
            return;
        }

        if (status == InputStatus.OnProcessing)
        {
            var itemInfoUI = GameManager.Instance.uiManager.itemInfo;

            if (itemInfoUI.IsActive == false
                 || itemInfoUI.CurrentID.Value != trigger.ItemID)
            {
                GameManager.Instance.uiManager.itemInfo.SetOnCursorBy(trigger.ItemID.Value);
            }
        }
        else
        {
            GameManager.Instance.uiManager.itemInfo.Disable();
        }
    }
    
    /// <summary>
    /// �� ���� ������ �����͸� �����ϴ� �Լ�
    /// </summary>
    private void SwapSlotItem(ItemSlot a, ItemSlot b)
    {
        ItemContainer bContainer = null;

        if (b.ItemID != null)
        {
            bContainer = Items[b.ItemID.Value];
        }

        if (a.ItemID != null)
        {
            b.Set(Items[a.ItemID.Value]);
        }
        else
        {
            b.Set(null);
        }

        a.Set(bContainer);
    }

    private bool GetEmptyBagItemSlot(out ItemSlot emptyBagSlot)
    {
        foreach (var bagItemSlot in bagItemSlots)
        {
            if (bagItemSlot.ItemID == null)
            {
                emptyBagSlot = bagItemSlot;
                return true;
            }
        }

        emptyBagSlot = null;
        return false;
    }

    /// <summary>
    /// �κ��丮 ���� ������ UI�� �����ϸ鼭 �۷ι� ������ �����͵� �����ش� �Լ�
    /// </summary>
    private void SetQuickSlot(int index, ItemContainer itemContainer)
    {
        quickItemSlots[index].Set(itemContainer);

        ExclusiveItemSlot quickSlot = GameManager.Instance.uiManager.quickSlot.GetQuickSlotByIndex(index);
        if (quickSlot != null)
        {
            quickSlot.Set(itemContainer); // ������ UI ������Ʈ
        }
    }
}