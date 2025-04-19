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
}
