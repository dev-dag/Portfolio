using UnityEngine;
using Database_Table;

public class ItemContainer
{
    public Item item;
    public int amount = 0;

    public virtual bool Init(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;

        return true;
    }

    public static ItemContainer CreateItemContainer(int id, int amount)
    {
        ItemContainer newContainer = null;

        if (GameManager.Instance.data.item.TryGetValue(id, out Item data))
        {
            newContainer = new ItemContainer();
            newContainer.Init(data, amount);
        }

        return newContainer;
    }
}
