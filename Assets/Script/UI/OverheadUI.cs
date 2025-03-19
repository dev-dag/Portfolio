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

    public void SetText(string text)
    {
        dialogTMP.text = text;
    }
}
