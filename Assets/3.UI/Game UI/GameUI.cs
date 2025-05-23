﻿using System;
using Database_Table;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : View
{
    public ObjectPool OverheadUI_Pool { get => overheadUI_Pool; }
    public Dialog Dialog { get => dialog; }
    public Inventory Inventory { get => inventory; }
    public QuickSlot QuickSlot { get => quickSlot; }
    public ItemInfo ItemInfo { get => itemInfo; }
    public PlayerInfoPreview PlayerInfoPreview { get => playerInfoPreview; }
    public SkillView SkillView { get => skillView; }


    [SerializeField] private ObjectPool overheadUI_Pool;
    [SerializeField] private Dialog dialog;
    [SerializeField] private Inventory inventory;
    [SerializeField] private QuickSlot quickSlot;
    [SerializeField] private ItemInfo itemInfo;
    [SerializeField] private PlayerInfoPreview playerInfoPreview;
    [SerializeField] private SkillView skillView;

    private void Start()
    {
        dialog.gameObject.SetActive(false);
        inventory.gameObject.SetActive(false);
        quickSlot.gameObject.SetActive(false);
        itemInfo.gameObject.SetActive(false);
        playerInfoPreview.gameObject.SetActive(false);
        skillView.gameObject.SetActive(false);
    }

    public override void Init()
    {
        dialog.Init();
        quickSlot.Init();
        inventory.Init();
        itemInfo.Init();
        playerInfoPreview.Init();
        skillView.Init();

        dialog.Hide();
        inventory.Hide();
        itemInfo.Hide();
        quickSlot.Show();
        playerInfoPreview.Show();
        skillView.Show();

        base.Init();
    }

    public void ShowUI_ForCinematic(bool isShown = false)
    {
        if (isShown)
        {
            quickSlot.Show();
            skillView.Show();
            playerInfoPreview.Show();
        }
        else
        {
            inventory.Hide();
            dialog.Hide();

            quickSlot.Hide();
            skillView.Hide();
            playerInfoPreview.Hide();
        }    
    }
}
