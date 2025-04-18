using Database_Table;
using UnityEngine;

public class LongSward : ItemContainer
{
    public override bool Init(Item item, int amount)
    {
        if (item.Type == 1)
        {
            return base.Init(item, amount);
        }
        else
        {
            return false;
        }
    }
}
