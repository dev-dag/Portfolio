public class ExclusiveItemSlot : ItemSlot
{
    public override void SetAmount(int amount)
    {
        itemAmount = amount;

        if (itemAmount >= 1)
        {
            amountText.text = itemAmount.ToString();
            SetAlpha(1f);
        }
        else
        {
            amountText.text = string.Empty;
            SetAlpha(0.5f);
        }

        iconImage.gameObject.SetActive(true);
        amountText.text = itemAmount.ToString();
    }
}
