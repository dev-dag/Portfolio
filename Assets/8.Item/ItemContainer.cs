using UnityEngine;
using Database_Table;
using System;

public class ItemContainer
{
    public event Action<ItemContainer> OnAmountChanged;

    private Item item;
    public Item Item
    {
        get => item;
        set
        {
            if (item != value)
            {
                item = value;
                OnAmountChanged?.Invoke(this);
            }
        }
    }

    private int amount = 0;
    public int Amount
    {
        get => amount;
        set
        {
            if (amount != value)
            {
                amount = value;
                OnAmountChanged?.Invoke(this);
            }
        }
    }

    public virtual bool Init(Item item, int amount)
    {
        this.Item = item;
        this.Amount = amount;

        return true;
    }

    public static ItemContainer CreateItemContainer(int id, int amount)
    {
        ItemContainer newContainer = null;

        if (GameManager.Instance.ReferenceData.item.TryGetValue(id, out Item data))
        {
            if (data.TypeEnum == ItemTypeEnum.Weapon)
            {
                newContainer = WeaponFactory.CreateWeapon((WeaponEnum)id);
            }
            else if (data.TypeEnum == ItemTypeEnum.Potion)
            {
                newContainer = new Potion();
            }
            
            newContainer.Init(data, amount);
        }

        return newContainer;
    }
}
