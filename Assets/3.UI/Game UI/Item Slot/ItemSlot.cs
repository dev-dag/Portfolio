using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : View, IPointerClickHandler, IDragHandler, IPointerMoveHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum InputStatus
    {
        OnProcessing = 0,
        Ended = 1,
    }

    public Sprite ItemIconSprite
    {
        get => iconImage.sprite;
    }

    protected int itemAmount = -1;
    public int ItemAmount
    {
        get => itemAmount;
    }

    public bool IsEmpty { get; protected set; } = true;
    public int ItemID { get; protected set; } = -1;
    public ItemTypeEnum ItemType { get; protected set; }

    public delegate void DragEventHandler(ItemSlot trigger, Vector2 position, InputStatus dragStatus);
    public delegate void ClickEventHandler(ItemSlot trigger, PointerEventData.InputButton button);
    public delegate void HoverEventHandler(ItemSlot trigger, Vector2 position, InputStatus dragStatus);

    [SerializeField] protected Image iconImage;
    [SerializeField] protected TMP_Text amountText;

    private DragEventHandler dragEventHandler;
    private ClickEventHandler clickEventHandler;
    private HoverEventHandler hoverEventHandler;
    private bool onDrag = false;

    private void Awake()
    {
        iconImage.gameObject.SetActive(false);
    }

    public override void Init()
    {
        base.Init();

        IsEmpty = true;
        ItemID = -1;
        ItemType = ItemTypeEnum.None;
        itemAmount = -1;

        this.dragEventHandler = null; 
        this.clickEventHandler = null;
        this.hoverEventHandler = null;

        onDrag = false;
        iconImage.sprite = null;
        iconImage.gameObject.SetActive(false);
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);
        amountText.text = string.Empty;
        amountText.alpha = 1f;
    }

    public void Init(DragEventHandler dragEventHandler, ClickEventHandler clickEventHandler, HoverEventHandler hoverEventHandler)
    {
        Init();

        this.dragEventHandler = dragEventHandler;
        this.clickEventHandler = clickEventHandler;
        this.hoverEventHandler = hoverEventHandler;
    }

    /// <summary>
    /// 드래그 이벤트
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onDrag = true;

            dragEventHandler?.Invoke(this, eventData.position, InputStatus.OnProcessing);
        }
    }

    /// <summary>
    /// 클릭 이벤트
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        clickEventHandler?.Invoke(this, eventData.button);
    }

    /// <summary>
    /// 마우스 호버 이벤트
    /// </summary>
    public void OnPointerMove(PointerEventData eventData)
    {
        if (onDrag)
        {
            return;
        }

        hoverEventHandler?.Invoke(this, eventData.position, InputStatus.OnProcessing);
    }

    /// <summary>
    /// 마우스 호버 종료 이벤트
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (onDrag)
        {
            return;
        }

        hoverEventHandler?.Invoke(this, eventData.position, InputStatus.Ended);
    }

    /// <summary>
    /// 드래그 종료 이벤트를 알리기 위한 이벤트
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (onDrag)
        {
            onDrag = false;

            dragEventHandler?.Invoke(this, eventData.position, InputStatus.Ended);
        }
    }

    public void Set(int itemID, int amount)
    {
        Init(dragEventHandler, clickEventHandler, hoverEventHandler); // 초기화

        if (GameManager.Instance.ReferenceData.item.TryGetValue(itemID, out var item)) // 아이템 정보를 찾아서 초기화
        {
            iconImage.sprite = item.IconSprite;
            iconImage.gameObject.SetActive(true);
            SetAmount(amount);
            ItemID = itemID;
            ItemType = item.TypeEnum;
            IsEmpty = false;
        }
    }

    public virtual void SetAmount(int amount)
    {
        itemAmount = amount;

        if (ItemAmount >= 1)
        {
            iconImage.gameObject.SetActive(true);
            amountText.text = ItemAmount.ToString();
        }
        else
        {
            iconImage.gameObject.SetActive(false);
            amountText.text = string.Empty;
        }
    }

    public void AddAmount(int amount)
    {
        SetAmount(itemAmount + amount);
    }

    public void SetAlpha(float alpha)
    {
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, alpha);
        amountText.alpha = alpha;
    }
}
