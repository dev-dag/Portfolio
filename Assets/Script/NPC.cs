using System;
using TMPro;
using UnityEngine;

public class NPC : BaseObject
{
    public bool IsInit { get; private set; } = false;

    [SerializeField] private Vector2 overheadUI_Offset;
    [SerializeField] private float overheadUI_Distance;
    [SerializeField] private float gKeyIconDistance;

    private OverheadUI overheadUI;

    protected override void Start()
    {
        base.Start();

        Init();
    }

    private void OnEnable()
    {
        if (IsInit)
        {
            CheckDistanceWithPlayer();
        }
    }

    private void MakeOverheadUI()
    {
        overheadUI = GameManager.Instance.uiManager.overheadUI_Pool.Burrow<OverheadUI>();
    }

    private void Init()
    {
        MakeOverheadUI();

        RectTransform overheadUI_RTR = overheadUI.GetComponent<RectTransform>();
        overheadUI_RTR.anchoredPosition = (Vector2)transform.position + overheadUI_Offset;

        overheadUI.SetText(GameManager.Instance.data.dialog[0].DialogText);
        overheadUI.Active(OverheadUI.Feature.ALL, false);

        CheckDistanceWithPlayer();

        IsInit = true;
    }

    /// <summary>
    /// �÷��̾�� �Ÿ��� üũ�ؼ� ���̾�α׸� ����ϰų� Ű���� �Է� ����� UI�� ����ϴ� �Լ�
    /// </summary>
    private async Awaitable CheckDistanceWithPlayer()
    {
        while (gameObject.activeSelf)
        {
            Vector2 distance = transform.position - Player.Current.transform.position;

            // ������� ���̾�α� �߻� ���� üũ
            if (Math.Abs(distance.x) < overheadUI_Distance && Math.Abs(distance.y) < overheadUI_Distance)
            {
                overheadUI.Active(OverheadUI.Feature.Dialog, true);
            }
            else
            {
                overheadUI.Active(OverheadUI.Feature.Dialog, false);
            }

            // G Key UI �߻� ���� üũ
            if (Math.Abs(distance.x) < gKeyIconDistance && Math.Abs(distance.y) < gKeyIconDistance)
            {
                overheadUI.Active(OverheadUI.Feature.GKeyIcon, true);
            }
            else
            {
                overheadUI.Active(OverheadUI.Feature.GKeyIcon, false);
            }

            await Awaitable.WaitForSecondsAsync(0.1f);
        }
    }
}
