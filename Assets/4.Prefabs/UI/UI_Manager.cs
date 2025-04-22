using Unity.VisualScripting;
using UnityEngine;

public class UI_Manager : BaseObject
{
    public ObjectPool overheadUI_Pool;
    public Dialog dialog;
    public Inventory inventory;
    public QuickSlot quickSlot;
    public ItemInfo itemInfo;
    public PlayerInfoPreview playerInfoPreview;

    protected override void Start()
    {
        base.Start();

        dialog.gameObject.SetActive(false);
        inventory.gameObject.SetActive(false);
        quickSlot.gameObject.SetActive(true);
        itemInfo.gameObject.SetActive(false);
        playerInfoPreview.gameObject.SetActive(true);
    }

    public void Init()
    {
        inventory.Init();
        playerInfoPreview.Init(5, null);

        inventory.AddItem(0, 1);
        inventory.AddItem(3, 3);
    }
}
