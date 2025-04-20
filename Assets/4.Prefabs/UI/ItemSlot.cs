using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : BaseObject, IPointerClickHandler, IDragHandler, IPointerMoveHandler, IPointerUpHandler
{
    public enum DragStatus
    {
        OnProcessing = 0,
        Ended = 1,
    }

    public delegate void DragEventHandler(ItemSlot trigger, Vector2 position, DragStatus dragStatus);
    public delegate void ClickEventHandler(ItemSlot trigger, PointerEventData.InputButton button);
    public delegate void HoverEventHandler(ItemSlot trigger, Vector2 position);

    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;

    public ItemContainer ItemContainer { get; private set; }

    private DragEventHandler dragEventHandler;
    private ClickEventHandler clickEventHandler;
    private HoverEventHandler hoverEventHandler;
    private bool onDrag = false;

    protected override void Awake()
    {
        base.Awake();

        if (ItemContainer == null)
        {
            iconImage.gameObject.SetActive(false);
        }
    }

    public void Init(DragEventHandler dragEventHandler, ClickEventHandler clickEventHandler, HoverEventHandler hoverEventHandler)
    {
        this.dragEventHandler = dragEventHandler;
        this.clickEventHandler = clickEventHandler;
        this.hoverEventHandler = hoverEventHandler;
    }

    /// <summary>
    /// 드래그 이벤트
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        onDrag = true;

        dragEventHandler?.Invoke(this, eventData.position, DragStatus.OnProcessing);

        EDebug.Log($"Drag : {eventData.position}");
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

        hoverEventHandler?.Invoke(this, eventData.position);

        EDebug.Log($"Hover : {eventData.position}");
    }

    /// <summary>
    /// 드래그 종료 이벤트를 알리기 위한 이벤트
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (onDrag)
        {
            onDrag = false;

            dragEventHandler?.Invoke(this, eventData.position, DragStatus.Ended);

            EDebug.Log($"End of Drag : {eventData.position}");
        }
    }

    public void Set(ItemContainer itemContainer)
    {
        this.ItemContainer = itemContainer;

        if (ItemContainer == null)
        {
            iconImage.gameObject.SetActive(false);
            amountText.text = string.Empty;
        }
        else
        {
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = ItemContainer.item.IconSprite;

            if (ItemContainer.amount > 1)
            {
                amountText.text = ItemContainer.amount.ToString();
            }
            else
            {
                amountText.text = string.Empty;
            }
        }    }

    /// <summary>
    /// 아이템의 수량을 증가시키는 함수
    /// </summary>
    public void AddAmount(int amount)
    {
        ItemContainer.amount += amount;
        
        if (ItemContainer.amount > 1)
        {
            amountText.text = ItemContainer.amount.ToString();
        }
        else
        {
            amountText.text = string.Empty;
        }
    }

    public void SetAlpha(float alpha)
    {
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, alpha);
        amountText.alpha = alpha;
    }
}
