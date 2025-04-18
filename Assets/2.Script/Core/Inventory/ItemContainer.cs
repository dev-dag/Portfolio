using UnityEngine;
using Database_Table;

public class ItemContainer
{
    private Item item;
    private int amount = 0;

    public virtual bool Init(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;

        return true;
    }
}
