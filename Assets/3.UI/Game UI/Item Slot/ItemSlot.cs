using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IPointerMoveHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum InputStatus
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

    public delegate void DragEventHandler(ItemSlot trigger, Vector2 position, InputStatus dragStatus);
    public delegate void ClickEventHandler(ItemSlot trigger, PointerEventData.InputButton button);
    public delegate void HoverEventHandler(ItemSlot trigger, Vector2 position, InputStatus dragStatus);

    [SerializeField] protected Image iconImage;
    [SerializeField] protected TMP_Text amountText;

    protected ItemContainer itemContainer;

    private DragEventHandler dragEventHandler;
    private ClickEventHandler clickEventHandler;
    private HoverEventHandler hoverEventHandler;
    private bool onDrag = false;

    private void Awake()
    {
        iconImage.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (itemContainer != null)
        {
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

        // UI 초기화
        OnContainerValueChange(itemContainer);
    }

    public void SetAlpha(float alpha)
    {
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, alpha);
        amountText.alpha = alpha;
    }

    /// <summary>
    /// UI의 상태를 컨테이너 값으로 갱신
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
