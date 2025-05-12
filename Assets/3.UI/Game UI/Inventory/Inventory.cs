using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ItemSlot;

public class Inventory : View
{
    public ItemSlot WeaponSlot { get => weaponSlot; }
    public int MaxSpace { get; private set; }
    public int RemainSpace { get; private set; } = 0;

    [Space(20f)]
    [SerializeField] private List<ItemSlot> bagItemSlots;
    [SerializeField] private ItemSlot weaponSlot;
    [SerializeField] private List<ExclusiveItemSlot> quickItemSlots;

    [Space(20f)]
    [SerializeField] private Image holdingItemImage;
    [SerializeField] private GraphicRaycaster raycaster;

    private bool onHolding = false;
    private Dictionary<int, ItemContainer> itemCache;

    /// <summary>
    /// 인벤토리 사용 전 호출되어야 하는 함수
    /// </summary>
    public override void Init()
    {
        base.Init();

        RemainSpace = 0;
        MaxSpace = GameManager.Instance.InstanceData.MaxItemSpace;
        itemCache = GameManager.Instance.InstanceData.Items; // 아이템 보유 정보를 캐싱
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

        Sync(); // 데이터와 싱크 맞추기
    }

    private void OnEnable()
    {
        if (IsInit == false)
        {
            return;
        }

        Sync(); // 데이터와 싱크 맞추기

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

        GameManager.Instance.gameUI.ItemInfo.Hide();
    }

    /// <summary>
    /// 아이템을 뷰에 추가하는 함수
    /// </summary>
    public bool AddItem(int itemID, int amount)
    {
        foreach (var quickSlot in quickItemSlots) // 퀵 슬롯에 있는 아이템인지 체크 후 독립적으로 수량 증가
        {
            if (quickSlot.ItemID == itemID)
            {
                quickSlot.AddAmount(amount);
                break;
            }
        }

        ItemSlot bagSlot = bagItemSlots.Find((arg) => arg.ItemID == itemID); // 인자 아이템 ID를 가진 슬롯 검색

        if (bagSlot != null) // 소지품에 있는 아이템인 경우 슬롯 뷰 수량 증가
        {
            bagSlot.AddAmount(amount);
            return true;
        }
        else if (weaponSlot.ItemID == itemID) // 인자 ID가 장착중인 무기인 경우 슬롯 뷰 수량 증가
        {
            weaponSlot.AddAmount(amount);

            return true;
        }
        else // 인벤토리 슬롯에 없는 아이템인 경우 슬롯 뷰에 아이템 세팅
        {
            if (GetEmptyBagItemSlot(out ItemSlot emptyBagSlot))
            {
                emptyBagSlot.Set(itemID, amount);

                RemainSpace--;
                return true;
            }
            else
            {
                return false; // 가방 공간 부족
            }
        }
    }

    /// <summary>
    /// 아이템을 뷰에서 제거하는 함수
    /// </summary>
    public bool RemoveItem(int itemID, int amount)
    {
        foreach (var quickSlot in quickItemSlots) // 퀵 슬롯에 있는 아이템인지 체크 후 독립적으로 수량 감소
        {
            if (quickSlot.ItemID == itemID)
            {
                quickSlot.AddAmount(-amount);
                break;
            }
        }

        ItemSlot bagSlot = bagItemSlots.Find((arg) => arg.ItemID == itemID); // 인자 아이템 ID를 가진 슬롯 검색

        if (bagSlot != null) // 소지품에 있는 아이템인 경우 슬롯 뷰 수량 감소. 
        {
            bagSlot.AddAmount(-amount); // 슬롯 뷰 수량 감소

            if (bagSlot.ItemAmount <= 0) // 잔여 수량이 0 이하인 경우 초기화.
            {
                if (GameManager.Instance.gameUI.ItemInfo.CurrentID != null && GameManager.Instance.gameUI.ItemInfo.CurrentID.Value == itemID)
                {
                    GameManager.Instance.gameUI.ItemInfo.Hide();
                }

                bagSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
                RemainSpace++;
            }

            return true;
        }
        else if (weaponSlot.ItemID == itemID) // 인자 ID가 장착중인 무기인 경우 슬롯 뷰 수량 감소. 
        {
            weaponSlot.AddAmount(-amount); // 슬롯 뷰 수량 감소

            if (weaponSlot.ItemAmount <= 0) // 잔여 수량이 0 이하인 경우 초기화.
            {
                weaponSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
                RemainSpace++;
            }

            return true;
        }
        else // 인벤토리 슬롯에 없는 아이템인 경우
        {
            return false;
        }
    }

    /// <summary>
    /// 슬롯 드래그 시 처리
    /// </summary>
    private void OnSlotDragging(ItemSlot trigger, Vector2 position, ItemSlot.InputStatus status)
    {
        if (trigger.IsEmpty)
        {
            return;
        }

        if (status == InputStatus.OnProcessing) // 드래그 중인 경우 처리
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
        }
        else if (status == InputStatus.Ended) // 드래그 종료 시 처리
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current) // 레이 캐스팅을 통해 해당 커서 위치에 아이템 슬롯 B 검색
            {
                position = Input.mousePosition
            };
            List<RaycastResult> resultList = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, resultList);

            if (resultList.Count >= 1)
            {
                ItemSlot dropSlot = resultList[0].gameObject.GetComponent<ItemSlot>();
                if (dropSlot != null)
                {
                    if (dropSlot == weaponSlot && trigger.ItemType == ItemTypeEnum.Weapon) // Drop이 무기 슬롯이고 Trigger가 무기 타입인 경우 스왑.
                    {
                        GameManager.Instance.InstanceData.EquippedWeaponID = trigger.ItemID; // 인스턴스 데이터에 장착된 무기 ID 갱신
                        SwapSlotItem(trigger, dropSlot);
                    }
                    else if (dropSlot is ExclusiveItemSlot && trigger.ItemType == ItemTypeEnum.Potion) // Drop이 퀵슬롯이고 Trigger가 포션 타입인 경우 스왑 시도
                    {
                        if (trigger is ExclusiveItemSlot) // 퀵슬롯 끼리 변경
                        {
                            int dropQuickSlotIndex = quickItemSlots.IndexOf(dropSlot as ExclusiveItemSlot);
                            bool dropSlotIsEmpty = dropSlot.IsEmpty;
                            int dropSlotItemID = dropSlot.ItemID;
                            int dropSlotItemAmount = dropSlot.ItemAmount;

                            if (trigger.IsEmpty)
                            {
                                SetQuickSlot(quickItemSlots.IndexOf(dropSlot as ExclusiveItemSlot));
                            }
                            else
                            {
                                SetQuickSlot(quickItemSlots.IndexOf(dropSlot as ExclusiveItemSlot), trigger.ItemID, trigger.ItemAmount);
                            }

                            int triggerQuickSlotIndex = quickItemSlots.IndexOf(trigger as ExclusiveItemSlot);
                            if (dropSlotIsEmpty)
                            {
                                SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot));
                            }
                            else
                            {
                                SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), dropSlotItemID, dropSlotItemAmount);
                            }
                        }
                        else // Drop은 퀵슬롯, Trigger는 아이템 슬롯인 경우
                        {
                            for (int index = 0; index < quickItemSlots.Count; index++) // 해당 아이템이 퀵 슬롯에 있는지 중복 체크
                            {
                                ExclusiveItemSlot quickItemSlot = quickItemSlots[index];

                                if (dropSlot != quickItemSlot && quickItemSlot.IsEmpty == false && quickItemSlot.ItemID == trigger.ItemID) // 옮기려는 슬롯은 서치 대상에서 제외
                                {
                                    SetQuickSlot(index);
                                    break;
                                }
                            }

                            SetQuickSlot(quickItemSlots.IndexOf(dropSlot as ExclusiveItemSlot), trigger.ItemID, trigger.ItemAmount); // 스왑 없이 퀵슬롯 설정
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
                SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), -1); // 퀵슬롯 해제
            }

            if (weaponSlot.IsEmpty) // 무기 장착 처리
            {
                Player.Current.EquipWeapon(null);
                GameManager.Instance.InstanceData.EquippedWeaponID = -1; // 인스턴스 데이터에 장착된 무기 ID 갱신
            }
            else
            {
                Player.Current.EquipWeapon(itemCache[weaponSlot.ItemID] as Weapon);
            }

            trigger.SetAlpha(1f);

            holdingItemImage.gameObject.SetActive(false);
            onHolding = false;
        }
    }

    /// <summary>
    /// 슬롯 클릭 시 처리
    /// </summary>
    private void OnSlotClicked(ItemSlot trigger, PointerEventData.InputButton button)
    {
        if (trigger.IsEmpty)
        {
            return;
        }
        else if (trigger is ExclusiveItemSlot
                    && trigger.ItemAmount <= 0) // 퀵 슬롯의 경우 수량이 0개일 때 상호작용 불가능한 상태가 있음.
        {
            SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), -1); // 해당 상태에서 우클릭 시 슬롯 캐싱까지 제거

            return;
        }

        if (button == PointerEventData.InputButton.Right) // 우클릭의 경우
        {
            if (trigger == weaponSlot) // trigger가 무기 슬롯인 경우
            {
                if (GetEmptyBagItemSlot(out ItemSlot emptyBagSlot))
                {
                    SwapSlotItem(trigger, emptyBagSlot);
                    GameManager.Instance.InstanceData.EquippedWeaponID = -1; // 인스턴스 데이터에 장착된 무기 ID 갱신
                }
            }
            else if (trigger is ExclusiveItemSlot) // trigger가 퀵 슬롯인 경우)
            {
                SetQuickSlot(quickItemSlots.IndexOf(trigger as ExclusiveItemSlot), -1);
            }
            else if (trigger.ItemType == ItemTypeEnum.Weapon) // trigger가 소지품 슬롯이고 무기 타입 아이템인 경우
            {
                SwapSlotItem(trigger, weaponSlot);
                GameManager.Instance.InstanceData.EquippedWeaponID = weaponSlot.ItemID; // 인스턴스 데이터에 장착된 무기 ID 갱신
            }
            else if (trigger.ItemType == ItemTypeEnum.Potion) // trigger가 소지품 슬롯이고 포션 타입 아이템인 경우
            {
                (itemCache[trigger.ItemID] as Potion).Drink();
                Sync();
            }

            if (weaponSlot.IsEmpty) // 무기 장착 처리
            {
                Player.Current.EquipWeapon(null);
                GameManager.Instance.InstanceData.EquippedWeaponID = -1; // 인스턴스 데이터에 장착된 무기 ID 갱신
            }
            else
            {
                Player.Current.EquipWeapon(itemCache[weaponSlot.ItemID] as Weapon);
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
        else if (trigger.IsEmpty) // 슬롯에 아이템이 없어도 반환.
        {
            return;
        }

        if (status == InputStatus.OnProcessing)
        {
            var itemInfoUI = GameManager.Instance.gameUI.ItemInfo;

            if (itemInfoUI.IsActive == false
                 || itemInfoUI.CurrentID.Value != trigger.ItemID)
            {
                GameManager.Instance.gameUI.ItemInfo.SetOnCursorBy(trigger.ItemID);
            }
        }
        else
        {
            GameManager.Instance.gameUI.ItemInfo.Hide();
        }
    }
    
    /// <summary>
    /// 두 개의 슬롯의 데이터를 변경하는 함수
    /// </summary>
    private void SwapSlotItem(ItemSlot a, ItemSlot b)
    {
        int tmpID = b.ItemID;
        int tmpAmount = b.ItemAmount;

        if (a.IsEmpty)
        {
            b.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }
        else
        {
            b.Set(a.ItemID, a.ItemAmount);
        }
            
        if (b.IsEmpty)
        {
            a.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }
        else
        {
            a.Set(tmpID, tmpAmount);
        }
    }

    private bool GetEmptyBagItemSlot(out ItemSlot emptyBagSlot)
    {
        emptyBagSlot = null;

        if (RemainSpace >= MaxSpace)
        {
            return false; // 가방 공간 부족.
        }

        foreach (var bagItemSlot in bagItemSlots)
        {
            if (bagItemSlot.IsEmpty)
            {
                emptyBagSlot = bagItemSlot;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 인벤토리 내의 퀵슬롯 UI를 설정하면서 글로벌 퀵슬롯 데이터도 갱신해는 함수
    /// </summary>
    private void SetQuickSlot(int index, int itemID = -1, int itemAmount = -1)
    {
        switch (index)
        {
            case 0:
                GameManager.Instance.InstanceData.QuickSlot_0_ID = itemID;
                break;
            case 1:
                GameManager.Instance.InstanceData.QuickSlot_1_ID = itemID;
                break;
            case 2:
                GameManager.Instance.InstanceData.QuickSlot_2_ID = itemID;
                break;
        }

        quickItemSlots[index].Set(itemID, itemAmount);

        GameManager.Instance.gameUI.QuickSlot.Sync();
    }

    /// <summary>
    /// 인스턴스 데이터와 뷰를 동기화하는 함수
    /// </summary>
    public void Sync()
    {
        foreach (var bagItemSlot in bagItemSlots) // 전체 슬롯 초기화
        {
            bagItemSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        weaponSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering); // 무기 슬롯 초기화

        foreach (ExclusiveItemSlot quickSlot in quickItemSlots) // 퀵 슬롯 초기화
        {
            quickSlot.Init(OnSlotDragging, OnSlotClicked, OnSlotHovering);
        }

        InstanceData data = GameManager.Instance.InstanceData;

        foreach (int itemID in data.Items.Keys) // 아이템 추가
        {
            if (itemID != data.EquippedWeaponID)
            {
                AddItem(itemID, itemCache[itemID].Amount);
                RemainSpace++;
            }
        }

        if (data.EquippedWeaponID != -1) // 무기 슬롯에 장착된 무기 설정
        {
            if (itemCache.ContainsKey(data.EquippedWeaponID))
            {
                weaponSlot.Set(data.EquippedWeaponID, itemCache[data.EquippedWeaponID].Amount);
            }
        }

        int slot_0_ID = GameManager.Instance.InstanceData.QuickSlot_0_ID;
        int slot_1_ID = GameManager.Instance.InstanceData.QuickSlot_1_ID;
        int slot_2_ID = GameManager.Instance.InstanceData.QuickSlot_2_ID;

        if (GameManager.Instance.InstanceData.Items.TryGetValue(slot_0_ID, out var container0)) // 0번 퀵슬롯 설정
        {
            quickItemSlots[0].Set(slot_0_ID, container0.Amount);
        }
        else
        {
            quickItemSlots[0].Set(slot_0_ID, 0);
        }

        if (GameManager.Instance.InstanceData.Items.TryGetValue(slot_1_ID, out var container1)) // 1번 퀵슬롯 설정
        {
            quickItemSlots[1].Set(slot_1_ID, container1.Amount);
        }
        else
        {
            quickItemSlots[1].Set(slot_1_ID, 0);
        }

        if (GameManager.Instance.InstanceData.Items.TryGetValue(slot_2_ID, out var container2)) // 2번 퀵슬롯 설정
        {
            quickItemSlots[2].Set(slot_2_ID, container2.Amount);
        }
        else
        {
            quickItemSlots[2].Set(slot_2_ID, 0);
        }
    }
}