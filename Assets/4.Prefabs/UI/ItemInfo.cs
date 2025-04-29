using Database_Table;
using TMPro;
using UnityEditor;
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
    private Canvas canvas;

    protected override void Awake()
    {
        base.Awake();

        canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Ŀ�� ��ġ�� Ư�� ������ ������ ����ϴ� �Լ�
    /// </summary>
    public async void SetOnCursorBy(int itemID)
    {
        current = GameManager.Instance.data.item[itemID];

        if (current.TypeEnum == Item.ItemType.Weapon)
        {
            WeaponInfo weaponInfo = GameManager.Instance.LoadItemInfo<WeaponInfo>(itemID);
            image.sprite = current.IconSprite;
            name.text = current.Name;
            type.text = "����";
            effect.text = $"���ݷ� +{weaponInfo.damage}";
            description.text = weaponInfo.description;

            this.gameObject.SetActive(true);
        }
        else if (current.TypeEnum == Item.ItemType.Potion)
        {
            PotionInfo potionInfo = GameManager.Instance.LoadItemInfo<PotionInfo>(itemID);
            image.sprite = current.IconSprite;
            name.text = current.Name;
            type.text = "����";
            effect.text = $"ȸ���� : +{potionInfo.healingAmount}";
            description.text = potionInfo.description;

            this.gameObject.SetActive(true);
        }

        SetPositionToCursorBased();
    }

    protected override void Update()
    {
        base.Update();

        SetPositionToCursorBased();
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }

    private void SetPositionToCursorBased()
    {
        RectTransform rtr = this.transform as RectTransform;

        // UI�� ������ ���� �õ�
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, Input.mousePosition, null, out Vector2 localAnchoredPos);
        rtr.pivot = new Vector2(0f, 0f);
        rtr.anchoredPosition = localAnchoredPos + Vector2.right * 100f;

        if (IsFullyInScreen() == false) // ������ ��ũ�� ������ ������ ���ʿ� ����
        {
            rtr.pivot = new Vector2(1f, 0f);
            rtr.anchoredPosition = localAnchoredPos + Vector2.left * 100f;
        }
    }

    private bool IsFullyInScreen()
    {
        RectTransform rtr = this.transform as RectTransform;

        Vector3[] corners = new Vector3[4];
        rtr.GetWorldCorners(corners);

        // ���ϴ�, ���� �ڳ� ���� ��ǥ ��ȸ
        Vector2 min = corners[0];
        Vector2 max = corners[0];

        foreach (Vector2 pos in corners)
        {
            if (min.x >= pos.x && min.y >= pos.y)
            {
                min = pos;
            }

            if (max.x <= pos.x && max.y <= pos.y)
            {
                max = pos;
            }
        }

        // ��ũ�� ���� ��ǥ�� ��ȯ
        Vector2 minScreenPos = RectTransformUtility.WorldToScreenPoint(null, min);
        Vector2 maxScreenPos = RectTransformUtility.WorldToScreenPoint(null, max);

        // ��ȿ���� ������ 0 ~ 1�� ������
        Vector2 normalizedScreenMinPos = new Vector2
        {
            x = minScreenPos.x / Screen.width,
            y = minScreenPos.y / Screen.height
        };

        Vector2 normalizedScreenMaxPos = new Vector2
        {
            x = maxScreenPos.x / Screen.width,
            y = maxScreenPos.y / Screen.height
        };

        // ��ȿ�� üũ
        if (normalizedScreenMinPos.x < 0f
            || normalizedScreenMaxPos.x > 1f) // ȭ�� ������ UI�� ����� ��� (�¿츸 ���)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
