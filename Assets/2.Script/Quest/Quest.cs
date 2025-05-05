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
    public State QuestState { get; protected set; } // 퀘스트 진행 상태
    public bool IsInit { get; protected set; } = false; // 초기화 여부 플래그

    protected Database_Table.Quest questData; // 퀘스트 데이터 테이블
    protected DataContainer db; // DB 커넥션 캐싱

    /// <summary>
    /// 퀘스트 수락 함수
    /// </summary>
    /// <returns>수락 조건에 충족하는 경우 True 반환</returns>
    public virtual bool TryAccept()
    {
        QuestState = State.OnProgress;

        return true;
    }

    /// <summary>
    /// 다이얼로그를 반환하는 함수
    /// </summary>
    /// <returns>진행 상황에 맞는 적절한 다이얼로그 텍스트 배열</returns>
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
    /// 오버헤드 다이얼로그를 반환하는 함수
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
        // DB 참조 초기화
        db = GameManager.Instance.ReferenceData;
        if (db == null)
        {
            return;
        }

        // 퀘스트 테이블 초기화
        if (db.quest.ContainsKey(QuestID) == false)
        {
            return;
        }
        questData = db.quest[QuestID];

        QuestState = State.OnStartable;

        IsInit = true;
    }

    /// <summary>
    /// 퀘스트 완료 보상을 지급하고 퀘스트를 완료처리하는 함수
    /// </summary>
    /// <returns></returns>
    public virtual bool ReceiveCompleteReward()
    {
        if (QuestState != State.OnComplete)
        {
            return false;
        }

        // 보상 획득 로직

        OnDone(); // 퀘스트 완료 처리

        return true;
    }

    /// <summary>
    /// 퀘스트 완료 조건을 만족한 상태일 때 호출되는 함수
    /// </summary>
    protected virtual void OnComplete()
    {
        QuestState = State.OnComplete;

        return;
    }
    
    /// <summary>
    /// 퀘스트를 종료하는 함수
    /// </summary>
    protected virtual void OnDone()
    {
        QuestState = State.Done;

        return;
    }
}
