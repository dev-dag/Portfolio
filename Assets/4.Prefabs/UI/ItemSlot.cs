using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : BaseObject, IPointerClickHandler, IDragHandler, IPointerMoveHandler
{
    public delegate void DragEventHandler(ItemSlot trigger, Vector2 position);
    public delegate void ClickEventHandler(ItemSlot trigger, PointerEventData.InputButton button);
    public delegate void HoverEventHandler(ItemSlot trigger, Vector2 position);

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text amountText;

    private ItemContainer itemContainer;

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

        dragEventHandler?.Invoke(this, eventData.position);

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

    public void Set(ItemContainer itemContainer, int amount)
    {
        this.itemContainer = itemContainer;

        // 아이템 이미지 불러오기
        amountText.text = amount.ToString();
    }
}
