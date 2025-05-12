using System.Collections.Generic;
using UnityEngine;

public class QuickSlot : View
{
    [SerializeField] private List<ExclusiveItemSlot> quickSlots;

    public override void Init()
    {
        base.Init();

        foreach (ExclusiveItemSlot quickSlot in quickSlots)
        {
            quickSlot.Init();
        }

        Sync();
    }

    /// <summary>
    /// 특정 아이템을 가지고 있는 슬롯을 반환
    /// </summary>
    public ExclusiveItemSlot GetQuickSlot(int holdingItemID)
    {
        foreach (ExclusiveItemSlot quickSlot in quickSlots)
        {
            if (quickSlot.IsEmpty == false && quickSlot.ItemID == holdingItemID)
            {
                return quickSlot;
            }
        }

        return null;
    }

    public ExclusiveItemSlot GetQuickSlotByIndex(int index)
    {
        return quickSlots[index];
    }

    /// <summary>
    /// 인스턴스 데이터와 뷰를 동기화하는 함수
    /// </summary>
    public void Sync()
    {
        int slot_0_ID = GameManager.Instance.InstanceData.QuickSlot_0_ID;
        int slot_1_ID = GameManager.Instance.InstanceData.QuickSlot_1_ID;
        int slot_2_ID = GameManager.Instance.InstanceData.QuickSlot_2_ID;

        if (GameManager.Instance.InstanceData.Items.TryGetValue(slot_0_ID, out var container0))
        {
            quickSlots[0].Set(slot_0_ID, container0.Amount);
        }
        else
        {
            quickSlots[0].Set(slot_0_ID, 0);
        }

        if (GameManager.Instance.InstanceData.Items.TryGetValue(slot_1_ID, out var container1))
        {
            quickSlots[1].Set(slot_1_ID, container1.Amount);
        }
        else
        {
            quickSlots[1].Set(slot_1_ID, 0);
        }

        if (GameManager.Instance.InstanceData.Items.TryGetValue(slot_2_ID, out var container2))
        {
            quickSlots[2].Set(slot_2_ID, container2.Amount);
        }
        else
        {
            quickSlots[2].Set(slot_2_ID, 0);
        }
    }
}
