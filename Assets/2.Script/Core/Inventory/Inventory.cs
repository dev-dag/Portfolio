using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory : BaseObject
{
    [SerializeField] private GraphicRaycaster raycaster;

    [Space(20f)]
    [SerializeField] private List<ItemSlot> bagItemSlotArr;
    [SerializeField] private ItemSlot weaponSlot;
    [SerializeField] private List<ItemSlot> quickItemSlotArr;

    [Space(20f)]
    [SerializeField] private Image holdingItemImage;
    private bool onHolding = false;

    /// <summary>
    /// �κ��丮 ��� �� ȣ��Ǿ�� �ϴ� �Լ�
    /// </summary>
    public void Init()
    {
        // ���� ������ ���� �ʱ�ȭ
        foreach (ItemSlot bagItemSlot in bagItemSlotArr)
        {
            bagItemSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        weaponSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering); // ���� ���� �ʱ�ȭ

        // ������ �ʱ�ȭ
        foreach (ItemSlot quickItemSlot in quickItemSlotArr)
        {
            quickItemSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        // �κ��丮 ����Ű ����
        InputActionMap UI_ActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("UI");
        UI_ActionMap.FindAction("Cancel").performed += (arg) => this.gameObject.SetActive(false);
        UI_ActionMap.FindAction("Inventory").performed += (arg) =>
        {
            if (this.gameObject.activeInHierarchy == false)
            {
                this.gameObject.SetActive(true);
            }
            else
            {
                this.gameObject.SetActive(false);
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
        playerActionMap.FindAction("UseSkill_1").Disable();
        playerActionMap.FindAction("UseSkill_2").Disable();
        playerActionMap.FindAction("UseSkill_3").Disable();
    }

    private void OnDisable()
    {
        InputActionMap playerActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("Player");
        playerActionMap.FindAction("UseSkill_1").Enable();
        playerActionMap.FindAction("UseSkill_2").Enable();
        playerActionMap.FindAction("UseSkill_3").Enable();
    }   
    /// <summary>
    /// �������� �߰��ϴ� �Լ�
    /// </summary>
    public void AddItem(int id, int amount)
    {
        bool isFind = false; // �˻� ���� ���θ� üũ�� �ο� ����

        // �� ���Կ��� id ��ȸ ��, ���� id�� �ִ� ��� ���� ����
        foreach (ItemSlot quickSlot in bagItemSlotArr)
        {
            if (quickSlot.ItemContainer?.item.ID == id)
            {
                quickSlot.AddAmount(1);
                isFind = true;
                break;
            }
        }

        if (isFind == false) // ����ǰ���� id ��ȸ ��, ���� id�� �ִ� ��� ���� ����
        {
            foreach (ItemSlot bagItemSlot in bagItemSlotArr)
            {
                if (bagItemSlot.ItemContainer?.item.ID == id)
                {
                    bagItemSlot.AddAmount(1);
                    isFind = true;
                    break;
                }
            }
        }

        if (isFind == false) // isFind == false�� ��� ������ �����̳ʸ� �����ؼ� ����ǰ�� �߰�
        {
            ItemContainer newItem = ItemContainer.CreateItemContainer(id, amount);

            foreach (ItemSlot bagItemSlot in bagItemSlotArr)
            {
                if (bagItemSlot.ItemContainer == null)
                {
                    bagItemSlot.Set(newItem);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// ���� �巡�� �� ó��
    /// </summary>
    private void OnSlotDragging(ItemSlot trigger, Vector2 position, ItemSlot.DragStatus dragStatus)
    {
        if (trigger.ItemContainer == null)
        {
            return;
        }

        switch (dragStatus)
        {
            case ItemSlot.DragStatus.OnProcessing: // �巡�� ���� ���
            {
                trigger.SetAlpha(0.5f); // trigger ���� 0.5f

                // Ȧ�� �̹��� ó��
                if (holdingItemImage.gameObject.activeInHierarchy == false)
                {
                    holdingItemImage.gameObject.SetActive(true);
                }

                holdingItemImage.sprite = trigger.ItemContainer.item.IconSprite; // holding image Ȱ��ȭ �� �����۰� ���� �̹����� ����

                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, position, null, out Vector2 pos);

                (holdingItemImage.transform as RectTransform).anchoredPosition = pos; // holding image position ����

                onHolding = true;

                break;
            }
            case ItemSlot.DragStatus.Ended: // �巡�� ���� �� ���
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
                            && trigger.ItemContainer.item.TypeEnum == Database_Table.Item.ItemType.Weapon) // trigger�� ���� Ÿ���� ��� ����.
                        {
                            SwapSlotItem(trigger, dropSlot);
                        }
                        else if (quickItemSlotArr.Contains(trigger) // Quick Slot ���� üũ
                            && trigger.ItemContainer.item.TypeEnum == Database_Table.Item.ItemType.Potion) // trigger�� ���� Ÿ���� ��� ����.
                        {
                            SwapSlotItem(trigger, dropSlot);
                        }
                        else
                        {
                            SwapSlotItem(trigger, dropSlot);
                        }
                    }
                }

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
        if (button == PointerEventData.InputButton.Right) // ��Ŭ���� ���
        {
            if (trigger == weaponSlot // trigger�� ���� ������ ���
                || quickItemSlotArr.Contains(trigger)) // trigger�� �� ������ ���
            {
                if (GetEmptyBagItemSlot(out ItemSlot emptyBagSlot))
                {
                    SwapSlotItem(trigger, emptyBagSlot);
                }
            }
            else if (trigger.ItemContainer.item.TypeEnum == Database_Table.Item.ItemType.Weapon) // trigger�� ����ǰ �����̰� ���� Ÿ�� �������� ���
            {
                SwapSlotItem(trigger, weaponSlot);
            }
        }
    }

    /// <summary>
    /// ���� ȣ���� �� ó��
    /// </summary>
    private void OnSlotHovering(ItemSlot trigger, Vector2 position)
    {
        if (onHolding) // �巡�� ���̸� �۵� ����.
        {
            return;
        }

        // Ŀ�� ��ġ�� ItemInfo�� �ѱ�.
    }

    private void SwapSlotItem(ItemSlot a, ItemSlot b)
    {
        ItemContainer bContainer = b.ItemContainer;

        if (quickItemSlotArr.Contains(b))
        {
            SetQuickSlot(quickItemSlotArr.IndexOf(b), a.ItemContainer);
        }
        else
        {
            b.Set(a.ItemContainer);
        }

        if (quickItemSlotArr.Contains(a))
        {
            SetQuickSlot(quickItemSlotArr.IndexOf(a), bContainer);
        }
        else
        {
            a.Set(bContainer);
        }
    }

    private bool GetEmptyBagItemSlot(out ItemSlot emptyBagSlot)
    {
        foreach (var bagItemSlot in bagItemSlotArr)
        {
            if (bagItemSlot.ItemContainer == null)
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
        quickItemSlotArr[index].Set(itemContainer);

        // ������ �ν��Ͻ� ������Ʈ
        if (index == 0)
        {
            GameManager.Instance.combatSystem.QuickItemContainer_1 = itemContainer;
        }
        else if (index == 1)
        {
            GameManager.Instance.combatSystem.QuickItemContainer_2 = itemContainer;
        }
        else if (index == 2)
        {
            GameManager.Instance.combatSystem.QuickItemContainer_3 = itemContainer;
        }
    }
}