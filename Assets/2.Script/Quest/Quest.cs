using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public abstract class Quest : BaseObject
{
    private const string OVERHEAD_TEXT_ON_PROGRESS = "...";
    private const string OVERHEAD_TEXT_ON_COMPLETE = "!";

    public enum State
    {
        None = 0,
        OnStartable = 1,
        OnProgress = 2,
        OnComplete = 3,
        Done = 4,
    }

    public int QuestID { get; protected set; }
    public State QuestState { get; protected set; } // ����Ʈ ���� ����
    public bool IsInit { get; protected set; } = false; // �ʱ�ȭ ���� �÷���

    protected Database_Table.Quest questData; // ����Ʈ ������ ���̺�
    protected DataContainer db; // DB Ŀ�ؼ� ĳ��

    /// <summary>
    /// ����Ʈ ���� �Լ�
    /// </summary>
    /// <returns>���� ���ǿ� �����ϴ� ��� True ��ȯ</returns>
    public virtual bool TryAccept()
    {
        QuestState = State.OnProgress;

        return true;
    }

    /// <summary>
    /// ���̾�α׸� ��ȯ�ϴ� �Լ�
    /// </summary>
    /// <returns>���� ��Ȳ�� �´� ������ ���̾�α� �ؽ�Ʈ �迭</returns>
    public virtual List<string> GetDialog()
    {
        switch (QuestState)
        {
            case State.OnStartable:
            {
                return db.dialog[questData.StartDialogID].DialogTextList;
            }
            case State.OnProgress:
            {
                return db.dialog[questData.ProcessDialogID].DialogTextList;
            }
            case State.OnComplete:
            {
                return db.dialog[questData.CompleteDialogID].DialogTextList;
            }
            default:
                return null;
        }
    }

    /// <summary>
    /// ������� ���̾�α׸� ��ȯ�ϴ� �Լ�
    /// </summary>
    /// <returns></returns>
    public virtual string GetOverheadDialog()
    {
        if (QuestState == State.OnStartable)
        {
            if (db.overheadDialog.TryGetValue(questData.OverheadDialogID, out var result))
            {
                return result.DialogText;
            }
        }
        else if (QuestState == State.OnProgress)
        {
            return "...";
        }
        
        switch (QuestState)
        {
            case State.OnStartable:
            {
                break;
            }
            case State.OnProgress:
            {
                return OVERHEAD_TEXT_ON_PROGRESS;
            }
            case State.OnComplete:
            {
                return OVERHEAD_TEXT_ON_COMPLETE;
            }
        }

        return null;
    }

    public virtual void Init()
    {
        // DB ���� �ʱ�ȭ
        db = GameManager.Instance.data;
        if (db == null)
        {
            return;
        }

        // ����Ʈ ���̺� �ʱ�ȭ
        if (db.quest.ContainsKey(QuestID) == false)
        {
            return;
        }
        questData = db.quest[QuestID];

        QuestState = State.OnStartable;

        IsInit = true;
    }

    /// <summary>
    /// ����Ʈ �Ϸ� ������ �����ϰ� ����Ʈ�� �Ϸ�ó���ϴ� �Լ�
    /// </summary>
    /// <returns></returns>
    public virtual bool ReceiveCompleteReward()
    {
        if (QuestState != State.OnComplete)
        {
            return false;
        }

        // ���� ȹ�� ����

        OnDone(); // ����Ʈ �Ϸ� ó��

        return true;
    }

    /// <summary>
    /// ����Ʈ �Ϸ� ������ ������ ������ �� ȣ��Ǵ� �Լ�
    /// </summary>
    protected virtual void OnComplete()
    {
        QuestState = State.OnComplete;

        return;
    }
    
    /// <summary>
    /// ����Ʈ�� �����ϴ� �Լ�
    /// </summary>
    protected virtual void OnDone()
    {
        QuestState = State.Done;

        return;
    }
}
