using UnityEngine;
using FluentBehaviourTree;
using System;
using Unity.Mathematics;

namespace Monster
{
    public class Boss : Monster, IInteractable
    {
        public enum Skill
        {
            None = 0,
            Slash,
            EjectSlash,
            Explosion,
            Rush,
        }

        public struct AnimHash
        {
            public static readonly int IDLE = Animator.StringToHash("Idle");
            public static readonly int MOVE = Animator.StringToHash("Move");
            public static readonly int DIE = Animator.StringToHash("Die");
            public static readonly int SLASH = Animator.StringToHash("Slash");
            public static readonly int EJECT_SLASH = Animator.StringToHash("Eject Slash");
            public static readonly int EXPLOSION = Animator.StringToHash("Explosion");
            public static readonly int RUSH = Animator.StringToHash("Rush");
            public static readonly int PHASE_EXIT_1 = Animator.StringToHash("Exit Phase 1");
        }

        [SerializeField] private Animator anim;
        [SerializeField] private Rigidbody2D rb;

        [Space(20f)]
        [SerializeField] private SkillData slashSkillData;
        [SerializeField] private SkillData ejectSlashSkillData;
        [SerializeField] private SkillData explosionSkillData;
        [SerializeField] private SkillData rushSkillData;

        private Transform playerTr;

        private IBehaviourTreeNode bt;
        private OverheadUI overheadUI;
        private int phase = 1;
        private float nextActionTime; // 스킬의 후딜레이를 지정하기 위해 사용함. 게임의 시간 값이 할당 됨.
        private Skill currentSkill = Skill.None;
        private int currentPlayingAnim;

        protected override void Awake()
        {
            base.Awake();

            bt = GetBehaviourTree();
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

        #region Behaviour Tree

        // 메인 Behaviour Tree 반환
        private IBehaviourTreeNode GetBehaviourTree()
        {
            var builder = new BehaviourTreeBuilder();

            // BT..
            builder.Selector(string.Empty)
                .Sequence(string.Empty) // 페이즈 체크
                    .Condition(string.Empty, t => hp <= 0f) // 체력이 0보다 작거나 같으면 페이즈 변경 시도
                    .Sequence(string.Empty)
                        .Do(string.Empty, t => // 페이즈 변경 시도
                        {
                            if (phase == 1)
                            {
                                phase = 2;
                                anim.Play(AnimHash.PHASE_EXIT_1);
                                currentPlayingAnim = AnimHash.PHASE_EXIT_1;
                            }

                            return BehaviourTreeStatus.Success;
                        })
                    .End()
                .End()

                .Sequence(string.Empty) // 메인 패턴 진입점
                    .Condition(string.Empty, t => Time.time > nextActionTime) // 스킬 사용 선제 조건 체크
                    .Sequence(string.Empty) // 준비중인 스킬이 없는 경우 랜덤하게 스킬 지정
                        .Do(string.Empty, t =>
                        {
                            if (currentSkill == Skill.None)
                            {
                                currentSkill = GetSkillRandomly();
                            }

                            return BehaviourTreeStatus.Success;
                        })
                    .End()

                    .Selector(string.Empty)
                        .Splice(GetSlashBehaviourTree())
                        .Splice(GetEjectSlashBehaviourTree())
                        .Splice(GetExplosionBehaviourTree())
                        .Splice(GetRushBehaviourTree())
                    .End()
                .End()

                .Do(string.Empty, t => // 플레이어를 바라보는 동작 처리.
                {
                    Vector3 dir = playerTr.position - transform.position;

                    // 좌우 회전
                    if (dir.x > 0f)
                    {
                        transform.rotation = Quaternion.identity;
                    }
                    else
                    {
                        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    }


                    anim.Play(AnimHash.IDLE);

                    return BehaviourTreeStatus.Success;
                })
            .End();

            return builder.Build();
        }

        /// <summary>
        /// 랜덤한 스킬을 반환하는 함수
        /// </summary>
        private Skill GetSkillRandomly()
        {
            System.Random random = new System.Random();

            int value = random.Next(0, 3);
            Skill result = Skill.None;

            switch (value)
            {
                case 0:
                {
                    result = Skill.Slash;
                    break;
                }
                case 1:
                {
                    result = Skill.EjectSlash;
                    break;
                }
                case 2:
                {
                    result = Skill.Explosion;
                    break;
                }
                case 3:
                {
                    result = Skill.Rush;
                    break;
                }
            }

            return result; // None이면 안됌.
        }

        // 공격 스킬 서브 트리 반환
        private IBehaviourTreeNode GetSlashBehaviourTree()
        {
            float afterDelay = 2f;
            float xDistance = 6f;

            BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

            builder.Sequence(string.Empty)
                .Condition(string.Empty, t => currentSkill == Skill.Slash)
                .Selector(string.Empty)
                    .Sequence(string.Empty)
                        .Condition(string.Empty, t => currentPlayingAnim == AnimHash.SLASH)
                        .Sequence(string.Empty) // 애니메이션 종료 체크
                            .Condition(string.Empty, t => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            .Do(string.Empty, t => // 애니메이션이 끝난 경우 후딜레이 설정
                            {
                                currentSkill = Skill.None;
                                nextActionTime = Time.time + afterDelay;

                                return BehaviourTreeStatus.Success;
                            })
                        .End()
                    .End()

                    .Selector(string.Empty)
                        .Sequence(string.Empty)
                            .Condition(string.Empty, t => playerTr.position.x - transform.position.x < xDistance)
                            .Do(string.Empty, t => // 공격 동작 실행
                            {
                                anim.Play(AnimHash.SLASH);
                                currentPlayingAnim = AnimHash.SLASH;

                                var skill = GameManager.Instance.combatSystem.GetSkill();
                                skill.Init(transform.position, transform.gameObject.layer, slashSkillData);
                                skill.Enable();

                                return BehaviourTreeStatus.Running;
                            })
                        .End()

                        .Do(string.Empty, t => // 플레이어쪽으로 이동
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            // 좌우 회전
                            if (dir.x > 0f)
                            {
                                transform.rotation = Quaternion.identity;
                            }
                            else
                            {
                                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                            }

                            rb.linearVelocityX = Mathf.Sign(dir.x) * info.speed;

                            anim.Play(AnimHash.MOVE);
                            currentPlayingAnim = AnimHash.MOVE;

                            return BehaviourTreeStatus.Running;
                        })
                    .End()
                .End()
            .End();

            return builder.Build();
        }

        // 공격 스킬 서브 트리 반환
        private IBehaviourTreeNode GetEjectSlashBehaviourTree()
        {
            float afterDelay = 2f;
            float xDistance = 60f;

            BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

            builder.Sequence(string.Empty)
                .Condition(string.Empty, t => currentSkill == Skill.EjectSlash)
                .Selector(string.Empty)
                    .Sequence(string.Empty)
                        .Condition(string.Empty, t => currentPlayingAnim == AnimHash.EJECT_SLASH)
                        .Sequence(string.Empty) // 애니메이션 종료 체크
                            .Condition(string.Empty, t => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            .Do(string.Empty, t => // 애니메이션이 끝난 경우 후딜레이 설정
                            {
                                currentSkill = Skill.None;
                                nextActionTime = Time.time + afterDelay;

                                return BehaviourTreeStatus.Success;
                            })
                        .End()
                    .End()

                    .Selector(string.Empty)
                        .Sequence(string.Empty)
                            .Condition(string.Empty, t => playerTr.position.x - transform.position.x < xDistance)
                            .Do(string.Empty, t => // 공격 동작 실행
                            {
                                anim.Play(AnimHash.EJECT_SLASH);
                                currentPlayingAnim = AnimHash.EJECT_SLASH;

                                var skill = GameManager.Instance.combatSystem.GetSkill();
                                skill.Init(transform.position, transform.gameObject.layer, ejectSlashSkillData);
                                skill.Enable();

                                return BehaviourTreeStatus.Running;
                            })
                        .End()

                        .Do(string.Empty, t => // 플레이어쪽으로 이동
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            // 좌우 회전
                            if (dir.x > 0f)
                            {
                                transform.rotation = Quaternion.identity;
                            }
                            else
                            {
                                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                            }

                            rb.linearVelocityX = Mathf.Sign(dir.x) * info.speed;

                            anim.Play(AnimHash.MOVE);
                            currentPlayingAnim = AnimHash.MOVE;

                            return BehaviourTreeStatus.Running;
                        })
                    .End()
                .End()
            .End();

            return builder.Build();
        }

        // 공격 스킬 서브 트리 반환
        private IBehaviourTreeNode GetExplosionBehaviourTree()
        {
            float afterDelay = 2f;
            float xDistance = 10f;

            BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

            builder.Sequence(string.Empty)
                .Condition(string.Empty, t => currentSkill == Skill.Explosion)
                .Selector(string.Empty)
                    .Sequence(string.Empty)
                        .Condition(string.Empty, t => currentPlayingAnim == AnimHash.EXPLOSION)
                        .Sequence(string.Empty) // 애니메이션 종료 체크
                            .Condition(string.Empty, t => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            .Do(string.Empty, t => // 애니메이션이 끝난 경우 후딜레이 설정
                            {
                                currentSkill = Skill.None;
                                nextActionTime = Time.time + afterDelay;

                                return BehaviourTreeStatus.Success;
                            })
                        .End()
                    .End()

                    .Selector(string.Empty)
                        .Sequence(string.Empty)
                            .Condition(string.Empty, t => playerTr.position.x - transform.position.x < xDistance)
                            .Do(string.Empty, t => // 공격 동작 실행
                            {
                                anim.Play(AnimHash.EXPLOSION);
                                currentPlayingAnim = AnimHash.EXPLOSION;

                                var skill = GameManager.Instance.combatSystem.GetSkill();
                                skill.Init(transform.position, transform.gameObject.layer, explosionSkillData);
                                skill.Enable();

                                return BehaviourTreeStatus.Running;
                            })
                        .End()

                        .Do(string.Empty, t => // 플레이어쪽으로 이동
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            // 좌우 회전
                            if (dir.x > 0f)
                            {
                                transform.rotation = Quaternion.identity;
                            }
                            else
                            {
                                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                            }

                            rb.linearVelocityX = Mathf.Sign(dir.x) * info.speed;

                            anim.Play(AnimHash.MOVE);
                            currentPlayingAnim = AnimHash.MOVE;

                            return BehaviourTreeStatus.Running;
                        })
                    .End()
                .End()
            .End();

            return builder.Build();
        }

        // 공격 스킬 서브 트리 반환
        private IBehaviourTreeNode GetRushBehaviourTree()
        {
            float afterDelay = 2f;
            float xDistance = 60f;

            BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

            builder.Sequence(string.Empty)
                .Condition(string.Empty, t => currentSkill == Skill.Rush)
                .Selector(string.Empty)
                    .Sequence(string.Empty)
                        .Condition(string.Empty, t => currentPlayingAnim == AnimHash.RUSH)
                        .Sequence(string.Empty) // 애니메이션 종료 체크
                            .Condition(string.Empty, t => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            .Do(string.Empty, t => // 애니메이션이 끝난 경우 후딜레이 설정
                            {
                                currentSkill = Skill.None;
                                nextActionTime = Time.time + afterDelay;

                                return BehaviourTreeStatus.Success;
                            })
                        .End()
                    .End()

                    .Selector(string.Empty)
                        .Sequence(string.Empty)
                            .Condition(string.Empty, t => playerTr.position.x - transform.position.x < xDistance)
                            .Do(string.Empty, t => // 공격 동작 실행
                            {
                                anim.Play(AnimHash.RUSH);
                                currentPlayingAnim = AnimHash.RUSH;

                                var skill = GameManager.Instance.combatSystem.GetSkill();
                                skill.Init(transform.position, transform.gameObject.layer, rushSkillData);
                                skill.Enable();

                                return BehaviourTreeStatus.Running;
                            })
                        .End()

                        .Do(string.Empty, t => // 플레이어쪽으로 이동
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            // 좌우 회전
                            if (dir.x > 0f)
                            {
                                transform.rotation = Quaternion.identity;
                            }
                            else
                            {
                                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                            }

                            rb.linearVelocityX = Mathf.Sign(dir.x) * info.speed;

                            anim.Play(AnimHash.MOVE);
                            currentPlayingAnim = AnimHash.MOVE;

                            return BehaviourTreeStatus.Running;
                        })
                    .End()
                .End()
            .End();

            return builder.Build();
        }

        #endregion
    }
}