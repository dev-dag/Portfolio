using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ItemSlot;

public class Inventory : View
{
    public Dictionary<int, ItemContainer> ItemContainers { get; private set; } = new Dictionary<int, ItemContainer>();
    public ItemSlot WeaponSlot { get => weaponSlot; }

    [Space(20f)]
    [SerializeField] private List<ItemSlot> bagItemSlots;
    [SerializeField] private ItemSlot weaponSlot;
    [SerializeField] private List<ExclusiveItemSlot> quickItemSlots;

    [Space(20f)]
    [SerializeField] private Image holdingItemImage;

    private GraphicRaycaster raycaster;
    private bool onHolding = false;

    /// <summary>
    /// 인벤토리 사용 전 호출되어야 하는 함수
    /// </summary>
    public override void Init()
    {
        base.Init();

        ItemContainers.Clear();
        raycaster = GetComponentInParent<GraphicRaycaster>();
        holdingItemImage.sprite = null;
        holdingItemImage.gameObject.SetActive(false);
        onHolding = false;

        // 가방 아이템 슬롯 초기화
        foreach (ItemSlot bagItemSlot in bagItemSlots)
        {
            bagItemSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        weaponSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering); // 무기 슬롯 초기화

        // 퀵슬롯 초기화
        foreach (ItemSlot quickItemSlot in quickItemSlots)
        {
            quickItemSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        // 인벤토리 단축키 설정
        InputActionMap UI_ActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("UI");
        UI_ActionMap.FindAction("Cancel").performed += (arg) => Hide();
        UI_ActionMap.FindAction("Inventory").performed += (arg) =>
        {
            if (Player.Current.OnInteration)
            {
                Hide();
            }
            else if (this.gameObject.activeInHierarchy == false)
            {
                Show();
            }
            else
            {
                Hide();
            }
        };

        // 데이터 기반으로 아이템 채우기
        {
            InstanceData data = GameManager.Instance.InstanceData;

            foreach (int itemID in data.Items.Keys) // 아이템 추가
            {
                if (itemID != data.EquippedWeaponID)
                {
                    AddItem(itemID, data.Items[itemID]);
                }
            }

            if (data.EquippedWeaponID != -1) // 무기 슬롯에 장착된 무기 설정
            {
                ItemContainer weaponContainer = ItemContainer.CreateItemContainer(data.EquippedWeaponID, 1);
                ItemContainers.Add(data.EquippedWeaponID, weaponContainer);
                weaponSlot.Set(weaponContainer);
            }

            if (data.QuickSlot_0_ID != -1) // 퀵 슬롯 0 설정
            {
                if (ItemContainers.ContainsKey(data.QuickSlot_0_ID))
                {
                    SetQuickSlot(0, ItemContainers[data.QuickSlot_0_ID]);
                }
            }

            if (data.QuickSlot_1_ID != -1) // 퀵 슬롯 1 설정
            {
                if (ItemContainers.ContainsKey(data.QuickSlot_1_ID))
                {
                    SetQuickSlot(1, ItemContainers[data.QuickSlot_1_ID]);
                }
            }

            if (data.QuickSlot_2_ID != -1) // 퀵 슬롯 2 설정
            {
                if (ItemContainers.ContainsKey(data.QuickSlot_2_ID))
                {
                    SetQuickSlot(2, ItemContainers[data.QuickSlot_2_ID]);
                }
            }
        }
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

        GameManager.Instance.gameUI.ItemInfo.Disable();
    }

    /// <summary>
    /// 아이템을 추가하는 함수
    /// </summary>
    public bool AddItem(int itemID, int amount)
    {
        if (ItemContainers.TryGetValue(itemID, out ItemContainer container))
        {
            container.Amount += amount;

            return true;
        }
        else
        {
            GameManager.Instance.LoadItemInfo<ItemInfoData>(itemID); // 아이템 정보 로드

            if (GetEmptyBagItemSlot(out ItemSlot emptyBagSlot))
            {
                ItemContainer newContainer = ItemContainer.CreateItemContainer(itemID, amount);
                newContainer.OnValueChanged += CheckItemValid;

                ItemContainers.Add(itemID, newContainer);
                emptyBagSlot.Set(newContainer);

                for (int index = 0; index < quickItemSlots.Count; index++)
                {
                    ExclusiveItemSlot quickItemSlot = quickItemSlots[index];

                    if (quickItemSlot.ItemID != null && quickItemSlot.ItemID == itemID)
                    {
                        if (quickItemSlot.ItemAmount == 0) // 소진된 아이템이 있는 경우 컨테이너 레퍼런스를 다시 설정해줘야 함.
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
                return false; // 가방 공간 부족
            }
        }
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }

    private void CheckItemValid(ItemContainer container)
    {
        if (container.Amount <= 0)
        {
            container.OnValueChanged -= CheckItemValid;
            ItemContainers.Remove(container.Item.ID);
        }
    }

    /// <summary>
    /// 슬롯 드래그 시 처리
    /// </summary>
    private void OnSlotDragging(ItemSlot trigger, Vector2 position, ItemSlot.InputStatus status)
    {
        if (trigger.ItemID == null
             || ItemContainers.ContainsKey(trigger.ItemID.Value) == false)
        {
            return;
        }

        switch (status)
        {
            case ItemSlot.InputStatus.OnProcessing: // 드래그 중인 경우
            {
                trigger.SetAlpha(0.5f); // trigger 알파 0.5f

                // 홀딩 이미지 처리
                if (holdingItemImage.gameObject.activeInHierarchy == false)
                {
                    holdingItemImage.gameObject.SetActive(true);
                }

                holdingItemImage.sprite = trigger.ItemIconSprite; // holding image 활성화 및 아이템과 같은 이미지로 변경

                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, position, null, out Vector2 pos);

                (holdingItemImage.transform as RectTransform).anchoredPosition = pos; // holding image position 변경

                onHolding = true;

                break;
            }
            case ItemSlot.InputStatus.Ended: // 드래그 종료 인 경우
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
                            && ItemContainers[trigger.ItemID.Value].Item.TypeEnum == Database_Table.Item.ItemType.Weapon) // trigger가 무기 타입인 경우 스왑.
                        {
                            SwapSlotItem(trigger, dropSlot);
                        }
                        else if (dropSlot is ExclusiveItemSlot // Quick Slot 인지 체크
                            && ItemContainers[trigger.ItemID.Value].Item.TypeEnum == Database_Table.Item.ItemType.Potion) // trigger가 포션 타입인 경우
                        {
                            if (trigger is ExclusiveItemSlot) // trigger도 퀵슬롯이변 퀵슬롯 간 변경
                            {
                                ItemContainer dropContainer = null;
                                if (dropSlot.ItemID != null)
                                {
                                    dropContainer = ItemContainers[dropSlot.ItemID.Value];
                                }

                                ItemContainer triggerContainer = null;
                                if (trigger.ItemID != null)
                                {
                                    triggerContainer = ItemContainers[trigger.ItemID.Value];
                                }

                                SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), dropContainer);
                                SetQuickSlot(quickItemSlots.IndexOf(dropSlot as ExclusiveItemSlot), triggerContainer);
                            }
                            else
                            {
                                for (int index = 0; index < quickItemSlots.Count; index++) // 해당 아이템이 퀵 슬롯에 있는지 중복 체크
                                {
                                    ExclusiveItemSlot quickItemSlot = quickItemSlots[index];

                                    if (dropSlot != quickItemSlot // 옮기려는 슬롯은 서치 대상에서 제외
                                        && quickItemSlot.ItemID != null && quickItemSlot.ItemID.Value == trigger.ItemID.Value)
                                    {
                                        SetQuickSlot(index, null);
                                        break;
                                    }
                                }

                                SetQuickSlot(quickItemSlots.IndexOf(dropSlot as ExclusiveItemSlot), ItemContainers[trigger.ItemID.Value]); // 스왑 없이 퀵슬롯 설정
                            }
                        }
                        else
                        {
                            SwapSlotItem(trigger, dropSlot);
                        }
                    }
                }
                else if (trigger is ExclusiveItemSlot) // 퀵슬롯을 드래그해서 허공에 버리는 제스쳐를 취하는 경우
                {
                    SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), null); // 퀵슬롯 해제
                }

                if (weaponSlot.ItemID == null) // 무기 장착 처리
                {
                    Player.Current.EquipWeapon(null);
                }
                else
                {
                    Player.Current.EquipWeapon(ItemContainers[weaponSlot.ItemID.Value] as Weapon);
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
        if (trigger.ItemID == null)
        {
            return;
        }
        else if (trigger is ExclusiveItemSlot
                    && trigger.ItemAmount.Value <= 0) // 퀵 슬롯의 경우 수량이 0개일 때 상호작용 불가능한 상태가 있음.
        {
            SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), null); // 해당 상태에서 우클릭 시 슬롯 캐싱까지 제거

            return;
        }

        ItemContainer container = ItemContainers[trigger.ItemID.Value];

        if (button == PointerEventData.InputButton.Right) // 우클릭의 경우
        {
            if (trigger == weaponSlot) // trigger가 무기 슬롯인 경우
            {
                if (GetEmptyBagItemSlot(out ItemSlot emptyBagSlot))
                {
                    SwapSlotItem(trigger, emptyBagSlot);
                }
            }
            else if (trigger is ExclusiveItemSlot) // trigger가 퀵 슬롯인 경우)
            {
                SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), null);
            }
            else if (container.Item.TypeEnum == Database_Table.Item.ItemType.Weapon) // trigger가 소지품 슬롯이고 무기 타입 아이템인 경우
            {
                SwapSlotItem(trigger, weaponSlot);
            }
            else if (container.Item.TypeEnum == Database_Table.Item.ItemType.Potion) // trigger가 소지품 슬롯이고 포션 타입 아이템인 경우
            {
                (container as Potion).Drink();
            }

            if (weaponSlot.ItemID == null) // 무기 장착 처리
            {
                Player.Current.EquipWeapon(null);
            }
            else
            {
                Player.Current.EquipWeapon(ItemContainers[weaponSlot.ItemID.Value] as Weapon);
            }
        }
    }

    /// <summary>
    /// 슬롯 호버링 시 처리
    /// </summary>
    private void OnSlotHovering(ItemSlot trigger, Vector2 position, InputStatus status)
    {
        if (onHolding) // 드래그 중이면 작동 안함.
        {
            return;
        }
        else if (trigger.ItemID == null) // 슬롯에 아이템이 없어도 반환.
        {
            return;
        }

        if (status == InputStatus.OnProcessing)
        {
            var itemInfoUI = GameManager.Instance.gameUI.ItemInfo;

            if (itemInfoUI.IsActive == false
                 || itemInfoUI.CurrentID.Value != trigger.ItemID)
            {
                GameManager.Instance.gameUI.ItemInfo.SetOnCursorBy(trigger.ItemID.Value);
            }
        }
        else
        {
            GameManager.Instance.gameUI.ItemInfo.Disable();
        }
    }
    
    /// <summary>
    /// 두 개의 슬롯의 데이터를 변경하는 함수
    /// </summary>
    private void SwapSlotItem(ItemSlot a, ItemSlot b)
    {
        ItemContainer bContainer = null;

        if (b.ItemID != null)
        {
            bContainer = ItemContainers[b.ItemID.Value];
        }

        if (a.ItemID != null)
        {
            b.Set(ItemContainers[a.ItemID.Value]);
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
    /// 인벤토리 내의 퀵슬롯 UI를 설정하면서 글로벌 퀵슬롯 데이터도 갱신해는 함수
    /// </summary>
    private void SetQuickSlot(int index, ItemContainer itemContainer)
    {
        quickItemSlots[index].Set(itemContainer);

        ExclusiveItemSlot quickSlot = GameManager.Instance.gameUI.QuickSlot.GetQuickSlotByIndex(index);
        if (quickSlot != null)
        {
            quickSlot.Set(itemContainer); // 퀵슬롯 UI 업데이트
        }
    }
}