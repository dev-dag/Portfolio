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
        /// BT를 만들어주는 함수
        /// </summary>
        /// <returns>BT의 루트 노트</returns>
        private IBehaviourTreeNode GetBT()
        {
            var builder = new BehaviourTreeBuilder();

            // BT..

            return builder.Build();
        }

        void IInteractable.CancelInteraction()
        {
            // IsInteractable == True 일 때만 호출 가능.
            // 대화 취소했을 때 처리. 다시 대화 가능하게 설정.
        }

        bool IInteractable.IsInteractable()
        {
            // 처음 조우 했을 때 한번 대화 가능.
        }

        void IInteractable.SetInteractionGuide(bool isActive)
        {
            // 상호작용 UI 노출
        }

        void IInteractable.StartInteraction(Action interactionCallback)
        {
            // IsInteractable == true 일 때만 호출 가능.
            // 다이얼로그 시작.
        }
    }
}