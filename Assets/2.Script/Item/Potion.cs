using Database_Table;
using UnityEngine;

public class Potion : ItemContainer
{
    private PotionInfo info;

    public override bool Init(Item item, int amount)
    {
        if (item.Type == 0)
        {
            info = GameManager.Instance.LoadItemInfo<PotionInfo>(item.ID);

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
        Player.Current.IncreaseHP(info.healingAmount);
        GameManager.Instance.audioSystem.PlaySFX(AudioSystem.AudioType.SFX, info.drinkSFX); // SFX 재생
    }
}
