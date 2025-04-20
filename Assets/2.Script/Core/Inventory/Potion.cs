using Database_Table;
using UnityEngine;

public class Potion : ItemContainer
{
    public override bool Init(Item item, int amount)
    {
        if (item.Type == 0)
        {
            return base.Init(item, amount);
        }
        else
        {
            return false;
        }
    }

    public void Drink()
    {
        Amount--;
        Player.Current.IncreaseHP(10);
    }
}
