using System.Collections.Generic;
using UnityEngine;

public class QuickSlot : BaseObject
{
    [SerializeField] private List<ExclusiveItemSlot> quickSlots;

    /// <summary>
    /// Ư�� �������� ������ �ִ� ������ ��ȯ
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
