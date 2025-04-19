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

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text amountText;

    public ItemContainer ItemContainer { get; private set; }

    private DragEventHandler dragEventHandler;
    private ClickEventHandler clickEventHandler;
    private HoverEventHandler hoverEventHandler;
    private bool onDrag = false;

    public void Init(DragEventHandler dragEventHandler, ClickEventHandler clickEventHandler, HoverEventHandler hoverEventHandler)
    {
        this.dragEventHandler = dragEventHandler;
        this.clickEventHandler = clickEventHandler;
        this.hoverEventHandler = hoverEventHandler;
    }

    protected override void Update()
    {
        base.Update();

        onDrag = false;
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
            dragEventHandler?.Invoke(this, eventData.position, DragStatus.Ended);

            EDebug.Log($"End of Drag : {eventData.position}");
        }
    }

    public void Set(ItemContainer itemContainer)
    {
        this.ItemContainer = itemContainer;

        // 아이템 이미지 불러오기
        amountText.text = ItemContainer.amount.ToString();
    }

    public void SetAlpha(float alpha)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        amountText.alpha = alpha;
    }
}
