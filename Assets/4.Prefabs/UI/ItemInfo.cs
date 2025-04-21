using Database_Table;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ItemInfo : BaseObject
{
    /// <summary>
    /// ���� �����ְ� �ִ� �������� ID��ȯ. ������ null ��ȯ
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
    /// ��������� ���� ��ȯ
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
    /// Ŀ�� ��ġ�� Ư�� ������ ������ ����ϴ� �Լ�
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
                    type.text = "����";
                    effect.text = $"���ݷ� +{result.Result.damage}";
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
                    type.text = "����";
                    effect.text = $"ȸ���� : +{result.Result.healingAmount}";
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
