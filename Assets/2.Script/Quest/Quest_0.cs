using UnityEngine;

/// <summary>
/// 테스트 퀘스트
/// </summary>
public class Quest_0 : Quest
{
    public Quest_0()
    {
        QuestID = 0;
    }

    public override bool TryAccept()
    {
        Timer();

        return base.TryAccept();
    }

    private async Awaitable Timer()
    {
        await Awaitable.WaitForSecondsAsync(5f);

        OnComplete();

        return;
    }
}