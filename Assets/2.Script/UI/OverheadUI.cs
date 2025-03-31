using System;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class OverheadUI : PoolingObject
{
    [SerializeField] private GameObject dialogObject;
    [SerializeField] private GameObject gKeyObject;

    [Flags]
    public enum Feature
    {
        None = 0,
        Dialog = 1,
        GKeyIcon = 2,
        ALL = 3,
    }

    public TMP_Text dialogTMP;

    public override void Enable()
    {
        base.Enable();

        Active(Feature.ALL, false);
    }

    public void Active(Feature feature, bool isActive)
    {
        int flag = (int)feature;

        if ((flag & (int)Feature.Dialog) > 0)
        {
            dialogObject.SetActive(isActive);
        }

        if ((flag & (int)Feature.GKeyIcon) > 0)
        {
            gKeyObject.SetActive(isActive);
        }
    }

    public bool IsActive(Feature feature)
    {
        int flag = (int)feature;

        if ((flag & (int)Feature.Dialog) > 0)
        {
            return dialogObject.activeInHierarchy;
        }

        if ((flag & (int)Feature.GKeyIcon) > 0)
        {
            return gKeyObject.activeInHierarchy;
        }

        return false;
    }

    public void ActiveDialog(bool isActive)
    {
        dialogObject.SetActive(isActive);
    }

    public void ActiveG_Key(bool isActive)
    {
        gKeyObject.SetActive(isActive);
    }

    public void SetDialogText(string text)
    {
        dialogTMP.text = text;
    }
}
