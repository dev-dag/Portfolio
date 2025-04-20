using Unity.VisualScripting;
using UnityEngine;

public class UI_Manager : BaseObject
{
    public ObjectPool overheadUI_Pool;
    public Dialog dialog;
    public Inventory inventory;

    protected override void Start()
    {
        base.Start();

        dialog.gameObject.SetActive(false);
        inventory.gameObject.SetActive(false);
    }

    public void Init()
    {
        inventory.Init();
    }
}
