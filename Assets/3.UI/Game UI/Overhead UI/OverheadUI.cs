using System;
using TMPro;
using UnityEngine;

public class OverheadUI : PoolingView
{
    private Vector3 offset;
    public Vector3 OffSet
    {
        get => offset;
        set
        {
            offset = value;

            rtr.anchoredPosition = origin + offset;
        }
    }

    private Vector3 origin;
    public Vector3 Origin
    {
        get => origin;
        set
        {
            origin = value;

            rtr.anchoredPosition = origin + offset;
        }
    }
    

    public Transform followTargetTr;

    [SerializeField] private GameObject dialogObject;
    [SerializeField] private GameObject gKeyObject;
    
    private RectTransform rtr;

    [Flags]
    public enum Feature
    {
        None = 0,
        Dialog = 1,
        GKeyIcon = 2,
        ALL = 3,
    }

    public TMP_Text dialogTMP;

    private void Awake()
    {
        rtr = GetComponent<RectTransform>();
    }

    public override void Init()
    {
        base.Init();

        this.followTargetTr = null;
        this.offset = Vector3.zero;
        this.origin = Vector3.zero;
        dialogObject.SetActive(false);
        dialogTMP.text = string.Empty;
        gKeyObject.SetActive(false);
        rtr.anchoredPosition = Vector2.zero;
    }

    public void Init(Transform followTargetTr, Vector3 offset)
    {
        Init();

        this.followTargetTr = followTargetTr;
        this.offset = offset;
    }

    public void Init(Vector3 origin, Vector3 offset)
    {
        Init();

        this.origin = origin;
        this.offset = offset;
    }

    public override void Enable()
    {
        base.Enable();

        Active(Feature.ALL, false);
    }

    public override void Return()
    {
        base.Return();

        followTargetTr = null;
        offset = Vector3.zero;
        origin = Vector3.zero;
    }

    private void Update()
    {
        if (followTargetTr != null)
        {
            rtr.anchoredPosition = followTargetTr.position + offset;
        }
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
