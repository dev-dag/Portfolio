using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestNPC : NPC
{
    private QuestNPC_Data QuestNPC_Data;

    protected override void Awake()
    {
        base.Awake();

        QuestNPC_Data = NPC_Data as QuestNPC_Data;
    }

    protected override void Init()
    {
        base.Init();

        if (QuestSystem.Instance == null
            || QuestSystem.Instance.IsInit == false)
        {
            EDebug.LogError("����Ʈ �ý����� �ʱ�ȭ���� ����.");
            IsInit = false;
            return;
        }
    }

    protected override bool HasDialog()
    {
        bool result = base.HasDialog();

        if (result)
        {
            return true;
        }

        foreach (int id in QuestNPC_Data.questID_List)
        {
            if (QuestSystem.Instance.quest.TryGetValue(id, out var quest))
            {
                if (quest.GetDialog() != null)
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }

    protected override bool HasOverheadDialog()
    {
        bool result = base.HasOverheadDialog();

        if (result)
        {
            return true;
        }

        foreach (int id in QuestNPC_Data.questID_List)
        {
            if (QuestSystem.Instance.quest.TryGetValue(id, out var quest))
            {
                if (quest.GetOverheadDialog() != null)
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }

    protected override void StartDialog()
    {
        if (GameManager.Instance.uiManager.dialog.IsActing)
        {
            return;
        }

        foreach (int id in QuestNPC_Data.questID_List)
        {
            if (QuestSystem.Instance.quest.TryGetValue(id, out var quest))
            {
                switch (quest.QuestState)
                {
                    case Quest.State.OnStartable:
                    {
                        GameManager.Instance.uiManager.dialog.StartDialog(quest.GetDialog(), () => quest.TryAccept());
                        return;
                    }
                    case Quest.State.OnComplete:
                    {
                        {
                            GameManager.Instance.uiManager.dialog.StartDialog(quest.GetDialog(), () => quest.ReceiveCompleteReward());
                            return;
                        }
                    }
                    case Quest.State.OnProgress:
                    {
                        GameManager.Instance.uiManager.dialog.StartDialog(quest.GetDialog());
                        return;
                    }
                }
            }
        }

        base.StartDialog();
    }

    protected override void SetOverheadDialog()
    {
        foreach (int id in QuestNPC_Data.questID_List)
        {
            if (QuestSystem.Instance.quest.TryGetValue(id, out var quest))
            {
                string text = quest.GetOverheadDialog();

                if (text != null)
                {
                    overheadUI.SetDialogText(quest.GetOverheadDialog());
                    return;
                }
            }
        }

        base.SetOverheadDialog();
        return;
    }
}
