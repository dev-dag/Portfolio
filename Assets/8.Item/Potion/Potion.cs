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
        GameManager.Instance.gameUI.QuickSlot.Sync(); // 퀵슬롯 UI 갱신
        Player.Current.IncreaseHP(info.healingAmount);
        GameManager.Instance.audioSystem.PlaySFX(AudioSystem.AudioType.SFX, info.drinkSFX); // SFX 재생

        if (GameManager.Instance.gameUI.Inventory.gameObject.activeInHierarchy)
        {
            GameManager.Instance.gameUI.Inventory.Sync();
        }
    }
}
