using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestSystem : SingleTon<QuestSystem>
{
    public bool IsInit { get; private set; } = false;

    public Dictionary<int, Quest> quest = new Dictionary<int, Quest>();

    public void Init()
    {
        if (GameManager.Instance == null
            || GameManager.Instance.ReferenceData == null
            || GameManager.Instance.ReferenceData.quest == null)
        {
            IsInit = false;
            return;
        }

        // 퀘스트 인스턴스 생성 및 초기화
        foreach (var table in GameManager.Instance.ReferenceData.quest.Values)
        {
            var instance = Activator.CreateInstance(Type.GetType(table.QuestClassName));
            
            if ((instance is Quest) == false)
            {
                EDebug.LogError("Quest 타입 오류 발견");
                continue;
            }

            Quest questInstance = instance as Quest;

            if (quest.ContainsKey(table.ID) == false)
            {
                quest.Add(table.ID, questInstance);

                questInstance.Init();
            }
        }

        IsInit = true;
    }
}
