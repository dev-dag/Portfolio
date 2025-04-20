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
    /// 인벤토리 사용 전 호출되어야 하는 함수
    /// </summary>
    public void Init()
    {
        // 가방 아이템 슬롯 초기화
        foreach (ItemSlot bagItemSlot in bagItemSlotArr)
        {
            bagItemSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        weaponSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering); // 무기 슬롯 초기화

        // 퀵슬롯 초기화
        foreach (ItemSlot quickItemSlot in quickItemSlotArr)
        {
            quickItemSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        // 인벤토리 단축키 설정
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
    /// 아이템을 추가하는 함수
    /// </summary>
    public void AddItem(int id, int amount)
    {
        bool isFind = false; // 검색 성공 여부를 체크할 부울 변수

        // 퀵 슬롯에서 id 조회 후, 같은 id가 있는 경우 수량 증가
        foreach (ItemSlot quickSlot in bagItemSlotArr)
        {
            if (quickSlot.ItemContainer?.item.ID == id)
            {
                quickSlot.AddAmount(1);
                isFind = true;
                break;
            }
        }

        if (isFind == false) // 소지품에서 id 조회 후, 같은 id가 있는 경우 수량 증가
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

        if (isFind == false) // isFind == false인 경우 아이템 컨테이너를 생성해서 소지품에 추가
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
    /// 슬롯 드래그 시 처리
    /// </summary>
    private void OnSlotDragging(ItemSlot trigger, Vector2 position, ItemSlot.DragStatus dragStatus)
    {
        if (trigger.ItemContainer == null)
        {
            return;
        }

        switch (dragStatus)
        {
            case ItemSlot.DragStatus.OnProcessing: // 드래그 중인 경우
            {
                trigger.SetAlpha(0.5f); // trigger 알파 0.5f

                // 홀딩 이미지 처리
                if (holdingItemImage.gameObject.activeInHierarchy == false)
                {
                    holdingItemImage.gameObject.SetActive(true);
                }

                holdingItemImage.sprite = trigger.ItemContainer.item.IconSprite; // holding image 활성화 및 아이템과 같은 이미지로 변경

                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, position, null, out Vector2 pos);

                (holdingItemImage.transform as RectTransform).anchoredPosition = pos; // holding image position 변경

                onHolding = true;

                break;
            }
            case ItemSlot.DragStatus.Ended: // 드래그 종료 인 경우
            {
                // 레이 캐스팅을 통해 해당 커서 위치에 아이템 슬롯 B 검색
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                List<RaycastResult> resultList = new List<RaycastResult>();

                raycaster.Raycast(pointerEventData, resultList);

                if (resultList.Count >= 1)
                {
                    ItemSlot dropSlot = resultList[0].gameObject.GetComponent<ItemSlot>();
                    // B 슬롯이 Bag의 아이템 슬롯인지 체크

                    if (dropSlot != null)
                    {
                        if (dropSlot == weaponSlot // Weapon Slot 인지 체크
                            && trigger.ItemContainer.item.TypeEnum == Database_Table.Item.ItemType.Weapon) // trigger가 무기 타입인 경우 스왑.
                        {
                            SwapSlotItem(trigger, dropSlot);
                        }
                        else if (quickItemSlotArr.Contains(trigger) // Quick Slot 인지 체크
                            && trigger.ItemContainer.item.TypeEnum == Database_Table.Item.ItemType.Potion) // trigger가 포션 타입인 경우 스왑.
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
    /// 슬롯 클릭 시 처리
    /// </summary>
    private void OnSlotClicked(ItemSlot trigger, PointerEventData.InputButton button)
    {
        if (button == PointerEventData.InputButton.Right) // 우클릭의 경우
        {
            if (trigger == weaponSlot // trigger가 무기 슬롯인 경우
                || quickItemSlotArr.Contains(trigger)) // trigger가 퀵 슬롯인 경우
            {
                if (GetEmptyBagItemSlot(out ItemSlot emptyBagSlot))
                {
                    SwapSlotItem(trigger, emptyBagSlot);
                }
            }
            else if (trigger.ItemContainer.item.TypeEnum == Database_Table.Item.ItemType.Weapon) // trigger가 소지품 슬롯이고 무기 타입 아이템인 경우
            {
                SwapSlotItem(trigger, weaponSlot);
            }
        }
    }

    /// <summary>
    /// 슬롯 호버링 시 처리
    /// </summary>
    private void OnSlotHovering(ItemSlot trigger, Vector2 position)
    {
        if (onHolding) // 드래그 중이면 작동 안함.
        {
            return;
        }

        // 커서 위치를 ItemInfo에 넘김.
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
    /// 인벤토리 내의 퀵슬롯 UI를 설정하면서 글로벌 퀵슬롯 데이터도 갱신해는 함수
    /// </summary>
    private void SetQuickSlot(int index, ItemContainer itemContainer)
    {
        quickItemSlotArr[index].Set(itemContainer);

        // 퀵슬롯 인스턴스 업데이트
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