using TMPro;
using UnityEngine;

public class NPC : BaseObject
{
    public TMP_Text overheadDialogTMP;

    protected override void Awake()
    {
        base.Awake();

        overheadDialogTMP.text = GameManager.Instance.data.dialog[0].DialogText;
    }
}
