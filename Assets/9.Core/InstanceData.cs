using System.Collections.Generic;

[System.Serializable]
public class InstanceData
{
    public Dictionary<int, int> Items { get; private set; } // 인벤토리 아이템 리스트 <Item ID, Amount>
    public int EquippedWeaponID { get; set; }
    public int QuickSlot_0_ID { get; set; }
    public int QuickSlot_1_ID { get; set; }
    public int QuickSlot_2_ID { get; set; }

    public InstanceData(Dictionary<int, int> items, int equippedWeaponID, int quickSlot_1_ID, int quickSlot_2_ID, int quickSlot_3_ID)
    {
        Items = new Dictionary<int, int>(items);
        
        EquippedWeaponID = equippedWeaponID;
        QuickSlot_0_ID = quickSlot_1_ID;
        QuickSlot_1_ID = quickSlot_2_ID;
        QuickSlot_2_ID = quickSlot_3_ID;
    }
}
