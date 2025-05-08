using System;
using UnityEngine;

/// <summary>
/// �÷��̾�� ��ȣ�ۿ��� �ؾ��� �� ����� �������̽�
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// �÷��̾�� ��ȣ�ۿ��� �����ϴ� �Լ�
    /// </summary>
    /// <param name="callback">��ȣ�ۿ��� ���� �� ȣ��Ǵ� �ݹ� �Լ�</param>
    public void StartInteraction(Action interactionCallback);

    /// <summary>
    /// ��ȣ�ۿ��� ����ϴ� �Լ�
    /// </summary>
    public void CancelInteraction();

    /// <summary>
    /// ��ȣ�ۿ� ������ �������� ���� ��ȯ
    /// </summary>
    public bool IsInteractable();

    /// <summary>
    /// ��ȣ�ۿ� ���̵� UI�� Ȱ��ȭ/��Ȱ��ȭ �ϴ� �Լ�
    /// </summary>
    public void SetInteractionGuide(bool isActive);
}
