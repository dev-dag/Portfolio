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
            || GameManager.Instance.data == null
            || GameManager.Instance.data.quest == null)
        {
            IsInit = false;
            return;
        }

        // ����Ʈ �ν��Ͻ� ���� �� �ʱ�ȭ
        foreach (var table in GameManager.Instance.data.quest.Values)
        {
            var instance = Activator.CreateInstance(Type.GetType(table.QuestClassName));
            
            if ((instance is Quest) == false)
            {
                EDebug.LogError("Quest Ÿ�� ���� �߰�");
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
