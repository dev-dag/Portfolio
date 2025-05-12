using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class InstanceData
{
    public Dictionary<int, ItemContainer> Items { get; private set; } = new Dictionary<int, ItemContainer>();
    public int EquippedWeaponID { get; set; }
    public int QuickSlot_0_ID { get; set; }
    public int QuickSlot_1_ID { get; set; }
    public int QuickSlot_2_ID { get; set; }
    public int MaxItemSpace { get; set; }
    public float BGM_Volume { get; private set; }
    public float SFX_Volume { get; private set; }
    public float UI_SFX_Volume { get; private set; }

    public InstanceData(Dictionary<int, int> items, int equippedWeaponID, int quickSlot_1_ID, int quickSlot_2_ID, int quickSlot_3_ID, int maxItemSpace, float bGM_Volume, float sFX_Volume, float uI_SFX_Volume)
    {
        EquippedWeaponID = equippedWeaponID;
        QuickSlot_0_ID = quickSlot_1_ID;
        QuickSlot_1_ID = quickSlot_2_ID;
        QuickSlot_2_ID = quickSlot_3_ID;
        MaxItemSpace = maxItemSpace;
        BGM_Volume = bGM_Volume;
        SFX_Volume = sFX_Volume;
        UI_SFX_Volume = uI_SFX_Volume;

        foreach (int itemID in items.Keys)
        {
            AddItem(itemID, items[itemID]);
        }
    }

    /// <summary>
    /// 아이템을 추가하는 함수
    /// </summary>
    public bool AddItem(int itemID, int amount)
    {
        if (Items.TryGetValue(itemID, out ItemContainer container))
        {
            container.Amount += amount;
        }
        else
        {
            if (Items.Count < MaxItemSpace)
            {
                if (GameManager.Instance.ReferenceData.item.TryGetValue(itemID, out var item))
                {
                    ItemContainer itemContainer = ItemContainer.CreateItemContainer(itemID, amount);
                    itemContainer.OnAmountChanged += OnAmountChanged;
                    Items.Add(itemID, itemContainer);
                }
                else
                {
                    EDebug.LogError("잘못된 데이터");

                    return false;
                }
            }
            else // 가방 여유공간이 없는 경우
            {
                return false;
            }
        }

        if (GameManager.Instance.gameUI?.QuickSlot.IsInit == true)
        {
            GameManager.Instance.gameUI.QuickSlot.Sync(); // 퀵슬롯 UI 동기화
        }

        if (GameManager.Instance.gameUI?.Inventory.IsInit == true && GameManager.Instance.gameUI.Inventory.gameObject.activeInHierarchy)
        {
            GameManager.Instance.gameUI.Inventory.Sync(); // 인벤토리 UI 동기화
        }

        return true;
    }

    /// <summary>
    /// 아이템을 삭제하는 함수
    /// </summary>
    public bool RemoveItem(int itemID, int amount)
    {
        if (Items.TryGetValue(itemID, out ItemContainer container))
        {
            container.Amount -= amount;

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// BGM 볼륨을 설정하는 함수
    /// </summary>
    /// <param name="volume"></param>
    public void SetBGM_Volume(float volume)
    {
        BGM_Volume = volume;

        if (GameManager.Instance.audioSystem.IsInit)
        {
            GameManager.Instance.audioSystem.SetVolume(AudioSystem.AudioType.BGM, volume);
        }
    }

    /// <summary>
    /// SFX 볼륨을 설정하는 함수
    /// </summary>
    public void SetSFX_Volume(float volume)
    {
        SFX_Volume = volume;

        if (GameManager.Instance.audioSystem.IsInit)
        {
            GameManager.Instance.audioSystem.SetVolume(AudioSystem.AudioType.SFX, volume);
        }
    }

    /// <summary>
    /// UI SFX 볼륨을 설정하는 함수
    /// </summary>
    public void SetUI_SFX_Volume(float volume)
    {
        UI_SFX_Volume = volume;

        if (GameManager.Instance.audioSystem.IsInit)
        {
            GameManager.Instance.audioSystem.SetVolume(AudioSystem.AudioType.UI_SFX, volume);
        }
    }

    /// <summary>
    /// 아이템 수량 변경 시 호출되는 함수
    /// </summary>
    private void OnAmountChanged(ItemContainer itemContainer)
    {
        if (Items.ContainsKey(itemContainer.Item.ID))
        {
            if (itemContainer.Amount <= 0)
            {
                Items[itemContainer.Item.ID] = null;
                Items.Remove(itemContainer.Item.ID);
            }
        }
    }
}
