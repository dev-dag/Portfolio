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
        private float nextActionTime; // ��ų�� �ĵ����̸� �����ϱ� ���� �����. ������ �ð� ���� �Ҵ� ��.
        private Skill currentSkill = Skill.None;
        private int currentPlayingAnim;

        protected override void Awake()
        {
            base.Awake();

            bt = GetBehaviourTree();
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

        #region Behaviour Tree

        // ���� Behaviour Tree ��ȯ
        private IBehaviourTreeNode GetBehaviourTree()
        {
            var builder = new BehaviourTreeBuilder();

            // BT..
            builder.Selector(string.Empty)
                .Sequence(string.Empty) // ������ üũ
                    .Condition(string.Empty, t => hp <= 0f) // ü���� 0���� �۰ų� ������ ������ ���� �õ�
                    .Sequence(string.Empty)
                        .Do(string.Empty, t => // ������ ���� �õ�
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

                .Sequence(string.Empty) // ���� ���� ������
                    .Condition(string.Empty, t => Time.time > nextActionTime) // ��ų ��� ���� ���� üũ
                    .Sequence(string.Empty) // �غ����� ��ų�� ���� ��� �����ϰ� ��ų ����
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

                .Do(string.Empty, t => // �÷��̾ �ٶ󺸴� ���� ó��.
                {
                    Vector3 dir = playerTr.position - transform.position;

                    // �¿� ȸ��
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
        /// ������ ��ų�� ��ȯ�ϴ� �Լ�
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

            return result; // None�̸� �ȉ�.
        }

        // ���� ��ų ���� Ʈ�� ��ȯ
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
                        .Sequence(string.Empty) // �ִϸ��̼� ���� üũ
                            .Condition(string.Empty, t => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            .Do(string.Empty, t => // �ִϸ��̼��� ���� ��� �ĵ����� ����
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
                            .Do(string.Empty, t => // ���� ���� ����
                            {
                                anim.Play(AnimHash.SLASH);
                                currentPlayingAnim = AnimHash.SLASH;

                                var skill = GameManager.Instance.combatSystem.GetSkill();
                                skill.Init(transform.position, transform.gameObject.layer, slashSkillData);
                                skill.Enable();

                                return BehaviourTreeStatus.Running;
                            })
                        .End()

                        .Do(string.Empty, t => // �÷��̾������� �̵�
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            // �¿� ȸ��
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

        // ���� ��ų ���� Ʈ�� ��ȯ
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
                        .Sequence(string.Empty) // �ִϸ��̼� ���� üũ
                            .Condition(string.Empty, t => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            .Do(string.Empty, t => // �ִϸ��̼��� ���� ��� �ĵ����� ����
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
                            .Do(string.Empty, t => // ���� ���� ����
                            {
                                anim.Play(AnimHash.EJECT_SLASH);
                                currentPlayingAnim = AnimHash.EJECT_SLASH;

                                var skill = GameManager.Instance.combatSystem.GetSkill();
                                skill.Init(transform.position, transform.gameObject.layer, ejectSlashSkillData);
                                skill.Enable();

                                return BehaviourTreeStatus.Running;
                            })
                        .End()

                        .Do(string.Empty, t => // �÷��̾������� �̵�
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            // �¿� ȸ��
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

        // ���� ��ų ���� Ʈ�� ��ȯ
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
                        .Sequence(string.Empty) // �ִϸ��̼� ���� üũ
                            .Condition(string.Empty, t => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            .Do(string.Empty, t => // �ִϸ��̼��� ���� ��� �ĵ����� ����
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
                            .Do(string.Empty, t => // ���� ���� ����
                            {
                                anim.Play(AnimHash.EXPLOSION);
                                currentPlayingAnim = AnimHash.EXPLOSION;

                                var skill = GameManager.Instance.combatSystem.GetSkill();
                                skill.Init(transform.position, transform.gameObject.layer, explosionSkillData);
                                skill.Enable();

                                return BehaviourTreeStatus.Running;
                            })
                        .End()

                        .Do(string.Empty, t => // �÷��̾������� �̵�
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            // �¿� ȸ��
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

        // ���� ��ų ���� Ʈ�� ��ȯ
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
                        .Sequence(string.Empty) // �ִϸ��̼� ���� üũ
                            .Condition(string.Empty, t => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                            .Do(string.Empty, t => // �ִϸ��̼��� ���� ��� �ĵ����� ����
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
                            .Do(string.Empty, t => // ���� ���� ����
                            {
                                anim.Play(AnimHash.RUSH);
                                currentPlayingAnim = AnimHash.RUSH;

                                var skill = GameManager.Instance.combatSystem.GetSkill();
                                skill.Init(transform.position, transform.gameObject.layer, rushSkillData);
                                skill.Enable();

                                return BehaviourTreeStatus.Running;
                            })
                        .End()

                        .Do(string.Empty, t => // �÷��̾������� �̵�
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            // �¿� ȸ��
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