using System;
using UnityEngine;

/// <summary>
/// 플레이어와 상호작용을 해야할 때 사용할 인터페이스
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 플레이어와 상호작용을 시작하는 함수
    /// </summary>
    /// <param name="callback">상호작용이 끝난 후 호출되는 콜백 함수</param>
    public void StartInteraction(Action interactionCallback);

    /// <summary>
    /// 상호작용을 취소하는 함수
    /// </summary>
    public void CancelInteraction();

    /// <summary>
    /// 상호작용 가능한 상태인지 여부 반환
    /// </summary>
    public bool IsInteractable();

    /// <summary>
    /// 상호작용 가이드 UI를 활성화/비활성화 하는 함수
    /// </summary>
    public void SetInteractionGuide(bool isActive);
}
