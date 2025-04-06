using UnityEngine;
using FluentBehaviourTree;
using System;

namespace Monster
{
    public class Boss : Monster, IInteractable
    {
        [SerializeField] private Animator anim;

        private IBehaviourTreeNode bt;
        private OverheadUI overheadUI;

        protected override void Awake()
        {
            base.Awake();

            bt = GetBT();
        }
        
        /// <summary>
        /// BT�� ������ִ� �Լ�
        /// </summary>
        /// <returns>BT�� ��Ʈ ��Ʈ</returns>
        private IBehaviourTreeNode GetBT()
        {
            var builder = new BehaviourTreeBuilder();

            // BT..

            return builder.Build();
        }

        void IInteractable.CancelInteraction()
        {
            // IsInteractable == True �� ���� ȣ�� ����.
            // ��ȭ ������� �� ó��. �ٽ� ��ȭ �����ϰ� ����.
        }

        bool IInteractable.IsInteractable()
        {
            // ó�� ���� ���� �� �ѹ� ��ȭ ����.
        }

        void IInteractable.SetInteractionGuide(bool isActive)
        {
            // ��ȣ�ۿ� UI ����
        }

        void IInteractable.StartInteraction(Action interactionCallback)
        {
            // IsInteractable == true �� ���� ȣ�� ����.
            // ���̾�α� ����.
        }
    }
}