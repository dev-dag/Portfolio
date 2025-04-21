using Database_Table;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ItemInfo : BaseObject
{
    /// <summary>
    /// 현재 보여주고 있는 아이템의 ID반환. 없으면 null 반환
    /// </summary>
    public int? CurrentID
    {
        get
        {
            if (current != null)
            {
                return current.ID;
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 사용중인지 여부 반환
    /// </summary>
    public bool IsActive
    {
        get
        {
            return this.gameObject.activeInHierarchy;
        }
    }

    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text type;
    [SerializeField] private TMP_Text effect;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Image image;

    private Item current;

    /// <summary>
    /// 커서 위치에 특정 아이템 인포를 출력하는 함수
    /// </summary>
    public async void SetOnCursorBy(int itemID)
    {
        current = GameManager.Instance.data.item[itemID];

        if (current.TypeEnum == Item.ItemType.Weapon)
        {
            Addressables.LoadAssetAsync<WeaponInfo>($"Item Info/{itemID}").Completed += result =>
            {
                if (result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    image.sprite = current.IconSprite;
                    name.text = current.Name;
                    type.text = "무기";
                    effect.text = $"공격력 +{result.Result.damage}";
                    description.text = result.Result.description;
                    
                    this.gameObject.SetActive(true);
                }
            };
        }
        else if (current.TypeEnum == Item.ItemType.Potion)
        {
            Addressables.LoadAssetAsync<PotionInfo>($"Item Info/{itemID}").Completed += result =>
            {
                if (result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    image.sprite = current.IconSprite;
                    name.text = current.Name;
                    type.text = "포션";
                    effect.text = $"회복량 : +{result.Result.healingAmount}";
                    description.text = result.Result.description;

                    this.gameObject.SetActive(true);
                }
            };
        }
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }
}
