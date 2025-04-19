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
    [SerializeField] private ItemSlot[] bagItemSlotArr;
    [SerializeField] private ItemSlot weaponSlot;
    [SerializeField] private ItemSlot[] quickItemSlotArr;

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
    }

    protected override void Start()
    {
        base.Start();

        Init();

        AddItem(3, 1);
        AddItem(0, 1);
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
                (holdingItemImage.transform as RectTransform).anchoredPosition = position; // holding image position ����

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
                        if (dropSlot == weaponSlot) // Weapon Slot ���� üũ
                        {
                            if (trigger.ItemContainer.item.TypeEnum == Database_Table.Item.ItemType.Weapon) // trigger�� ���� Ÿ���� ��� ����.
                            {
                                SwapSlotItem(trigger, dropSlot);
                            }
                        }
                        else if (dropSlot == quickItemSlotArr[0] // Quick Slot ���� üũ
                                || dropSlot == quickItemSlotArr[1]
                                || dropSlot == quickItemSlotArr[2])
                        {
                            if (trigger.ItemContainer.item.TypeEnum == Database_Table.Item.ItemType.Potion) // trigger�� ���� Ÿ���� ��� ����.
                            {
                                SwapSlotItem(trigger, dropSlot);
                            }
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
            if (trigger.ItemContainer.item.TypeEnum == Database_Table.Item.ItemType.Weapon)
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

        b.Set(a.ItemContainer);
        a.Set(bContainer);
    }
}