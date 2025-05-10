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
    }

    /// <summary>
    /// 특정 아이템을 가지고 있는 슬롯을 반환
    /// </summary>
    public ExclusiveItemSlot GetQuickSlot(int holdingItemID)
    {
        foreach (ExclusiveItemSlot quickSlot in quickSlots)
        {
            if (quickSlot.ItemID != null
                && quickSlot.ItemID.Value == holdingItemID)
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
}
