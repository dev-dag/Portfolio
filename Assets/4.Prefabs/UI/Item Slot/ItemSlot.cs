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

    public int? ItemID
    {
        get
        {
            if (itemContainer != null)
            {
                return itemContainer.Item.ID;
            }
            else
            {
                return null;
            }
        }
    }

    public int? ItemAmount
    {
        get
        {
            if (itemContainer != null)
            {
                return itemContainer.Amount;
            }
            else
            {
                return null;
            }
        }
    }

    public Sprite ItemIconSprite
    {
        get
        {
            if (itemContainer != null)
            {
                return itemContainer.Item.IconSprite;
            }
            else
            {
                return null;
            }
        }
    }

    public delegate void DragEventHandler(ItemSlot trigger, Vector2 position, DragStatus dragStatus);
    public delegate void ClickEventHandler(ItemSlot trigger, PointerEventData.InputButton button);
    public delegate void HoverEventHandler(ItemSlot trigger, Vector2 position);

    [SerializeField] protected Image iconImage;
    [SerializeField] protected TMP_Text amountText;

    protected ItemContainer itemContainer;

    private DragEventHandler dragEventHandler;
    private ClickEventHandler clickEventHandler;
    private HoverEventHandler hoverEventHandler;
    private bool onDrag = false;

    protected override void Awake()
    {
        base.Awake();
        iconImage.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (itemContainer != null)
        {
            if (iconImage.sprite == null)
            {
                iconImage.sprite = itemContainer.Item.IconSprite; // �κ��丮 �ʱ�ȭ Ÿ�ֿ̹� �̹����� �޸𸮿� �ö���� ���� ���, Enable���� �̹��� ����
            }
            
            itemContainer.OnValueChanged -= OnContainerValueChange;
            itemContainer.OnValueChanged += OnContainerValueChange;

            OnContainerValueChange(itemContainer);
        }
    }

    private void OnDisable()
    {
        if (itemContainer != null)
        {
            itemContainer.OnValueChanged -= OnContainerValueChange;
        }
    }

    public void Init(DragEventHandler dragEventHandler, ClickEventHandler clickEventHandler, HoverEventHandler hoverEventHandler)
    {
        this.dragEventHandler = dragEventHandler;
        this.clickEventHandler = clickEventHandler;
        this.hoverEventHandler = hoverEventHandler;
    }

    /// <summary>
    /// �巡�� �̺�Ʈ
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onDrag = true;

            dragEventHandler?.Invoke(this, eventData.position, DragStatus.OnProcessing);

            EDebug.Log($"Drag : {eventData.position}");
        }
    }

    /// <summary>
    /// Ŭ�� �̺�Ʈ
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        clickEventHandler?.Invoke(this, eventData.button);
    }

    /// <summary>
    /// ���콺 ȣ�� �̺�Ʈ
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
    /// �巡�� ���� �̺�Ʈ�� �˸��� ���� �̺�Ʈ
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

    public void Set(ItemContainer newContainer)
    {
        if (itemContainer != null)
        {
            itemContainer.OnValueChanged -= OnContainerValueChange;
        }

        itemContainer = newContainer;

        if (itemContainer != null)
        {
            itemContainer.OnValueChanged += OnContainerValueChange;
            iconImage.sprite = itemContainer.Item.IconSprite;
        }

        // UI �ʱ�ȭ
        OnContainerValueChange(itemContainer);
    }

    public void SetAlpha(float alpha)
    {
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, alpha);
        amountText.alpha = alpha;
    }

    /// <summary>
    /// UI�� ���¸� �����̳� ������ ����
    /// </summary>
    protected virtual void OnContainerValueChange(ItemContainer changed)
    {
        if (itemContainer == null)
        {
            iconImage.gameObject.SetActive(false);
            amountText.text = string.Empty;
        }
        else if (itemContainer.Amount >= 1)
        {
            iconImage.gameObject.SetActive(true);

            if (itemContainer.Item.TypeEnum == Database_Table.Item.ItemType.Potion)
            {
                amountText.text = itemContainer.Amount.ToString();
            }
        }
        else
        {
            iconImage.gameObject.SetActive(false);
            amountText.text = string.Empty;
            itemContainer = null;
        }
    }
}
