using Database_Table;
using TMPro;
using UnityEditor;
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
    private Canvas canvas;

    protected override void Awake()
    {
        base.Awake();

        canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// 커서 위치에 특정 아이템 인포를 출력하는 함수
    /// </summary>
    public async void SetOnCursorBy(int itemID)
    {
        current = GameManager.Instance.data.item[itemID];

        if (current.TypeEnum == Item.ItemType.Weapon)
        {
            WeaponInfo weaponInfo = GameManager.Instance.LoadItemInfo<WeaponInfo>(itemID);
            image.sprite = current.IconSprite;
            name.text = current.Name;
            type.text = "무기";
            effect.text = $"공격력 +{weaponInfo.damage}";
            description.text = weaponInfo.description;

            this.gameObject.SetActive(true);
        }
        else if (current.TypeEnum == Item.ItemType.Potion)
        {
            PotionInfo potionInfo = GameManager.Instance.LoadItemInfo<PotionInfo>(itemID);
            image.sprite = current.IconSprite;
            name.text = current.Name;
            type.text = "포션";
            effect.text = $"회복량 : +{potionInfo.healingAmount}";
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

        // UI를 우측에 노출 시도
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, Input.mousePosition, null, out Vector2 localAnchoredPos);
        rtr.pivot = new Vector2(0f, 0f);
        rtr.anchoredPosition = localAnchoredPos + Vector2.right * 100f;

        if (IsFullyInScreen() == false) // 우측에 스크린 공간이 없으면 왼쪽에 노출
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

        // 좌하단, 우상단 코너 월드 좌표 조회
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

        // 스크린 기준 좌표로 변환
        Vector2 minScreenPos = RectTransformUtility.WorldToScreenPoint(null, min);
        Vector2 maxScreenPos = RectTransformUtility.WorldToScreenPoint(null, max);

        // 유효값의 범위를 0 ~ 1로 리매핑
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

        // 유효성 체크
        if (normalizedScreenMinPos.x < 0f
            || normalizedScreenMaxPos.x > 1f) // 화면 범위를 UI가 벗어나는 경우 (좌우만 계산)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
