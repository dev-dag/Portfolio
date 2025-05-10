public class ExclusiveItemSlot : ItemSlot
{
    protected override void OnContainerValueChange(ItemContainer changed)
    {
        if (itemContainer == null)
        {
            iconImage.gameObject.SetActive(false);
            amountText.text = string.Empty;
        }
        else if (itemContainer.Amount >= 1)
        {
            iconImage.gameObject.SetActive(true);

            if (itemContainer.Item.TypeEnum == ItemTypeEnum.Potion)
            {
                amountText.text = itemContainer.Amount.ToString();
            }

            SetAlpha(1f);
        }
        else
        {
            iconImage.gameObject.SetActive(true);

            if (itemContainer.Item.TypeEnum == ItemTypeEnum.Potion)
            {
                amountText.text = itemContainer.Amount.ToString();
            }

            SetAlpha(0.5f);
        }
    }
}
