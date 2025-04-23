using Database_Table;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LongSward : ItemContainer
{
    public WeaponInfo WeaponInfo { get; private set; }

    public override bool Init(Item item, int amount)
    {
        Addressables.LoadAssetAsync<WeaponInfo>($"Item Info/{item.ID}").Completed += result =>
        {
            if (result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                WeaponInfo = result.Result;
            }
        };

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
