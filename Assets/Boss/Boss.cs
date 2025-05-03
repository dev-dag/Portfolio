using UnityEngine;
using FluentBehaviourTree;
using System;

namespace Monster
{
    public class Boss : Monster, IInteractable, ICombatable
    {
        public enum SkillState
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
        [SerializeField] private SpriteRenderer render;

        [Space(20f)]
        [SerializeField] private SkillData slashSkillData;
        [SerializeField] private SkillData ejectSlashSkillData;
        [SerializeField] private SkillData explosionSkillData;
        [SerializeField] private SkillData rushSkillData;

        [SerializeField] private Transform playerTr;

        private IBehaviourTreeNode bt;
        private OverheadUI overheadUI;
        private int phase = 1;
        private float nextActionTime; // ��ų�� �ĵ����̸� �����ϱ� ���� �����. ������ �ð� ���� �Ҵ� ��.
        private SkillState currentSkill = SkillState.None;
        private int currentPlayingAnim;
        private bool isInteractable = true;
        private Awaitable colorFadeAwaiter = null;
        private float colorFadeTime = 0.2f;

        private Skill slashSkill;
        private Skill ejectSlashSkill;
        private Skill explosionSkill;
        private Skill rushSkill;

        protected override void Awake()
        {
            base.Awake();

            Init();

            this.gameObject.layer = LayerMask.NameToLayer(GameManager.INTERACTABLE_OBJECT_LAYER_NAME);
            if (this.gameObject.layer == -1)
            {
                EDebug.LogError("���̾� �̸� ����");
            }

            bt = GetBehaviourTree();
        }

        protected override void Start()
        {
            base.Start();

            overheadUI = GameManager.Instance.uiManager.overheadUI_Pool.Burrow<OverheadUI>();
            overheadUI.Init(this.transform, Vector3.zero);
            overheadUI.Enable();

            rb.bodyType = RigidbodyType2D.Kinematic; // ù ���� �� ��ȣ�ۿ� ���� �� ��ġ���� ��Ż ����
        }

        protected override void Update()
        {
            base.Update();

            if (playerTr != null)
            {
                bt.Tick(new TimeData(Time.deltaTime));
            }
        }

        void ICombatable.TakeHit(int damage, BaseObject hitter)
        {
            if (isInteractable)
            {
                return;
            }

            hp -= damage;

            TakeHitVFX vfx = GameManager.Instance.combatSystem.GetTakeHitVFX();
            vfx.Init(this.transform.position + Vector3.down * 1f);
            vfx.Enable();

            if (colorFadeAwaiter != null)
            {
                colorFadeAwaiter.Cancel();
                colorFadeAwaiter = null;
            }

            colorFadeAwaiter = FadeColor();
        }
        
        private async Awaitable FadeColor()
        {
            float timer = Time.time + colorFadeTime;

            render.color = new Color(1f, 0f, 0f, 1f);

            while (Time.time < timer)
            {
                float color = Mathf.Lerp(0f, 1f, 1 - ((timer - Time.time) / colorFadeTime));
                render.color = new Color(1f, color, color, 1f);

                await Awaitable.NextFrameAsync();
            }

            render.color = new Color(1f, 1f, 1f, 1f);
        }

        void IInteractable.CancelInteraction()
        {
            // IsInteractable == True �� ���� ȣ�� ����.
            // ��ȭ ������� �� ó��. �ٽ� ��ȭ �����ϰ� ����.
            isInteractable = true;
        }

        bool IInteractable.IsInteractable()
        {
            // ó�� ���� ���� �� �ѹ� ��ȭ ����.
            return isInteractable;
        }

        void IInteractable.SetInteractionGuide(bool isActive)
        {
            // ��ȣ�ۿ� UI ����
            overheadUI.ActiveG_Key(isActive);
        }

        void IInteractable.StartInteraction(Action interactionCallback)
        {
            // IsInteractable == true �� ���� ȣ�� ����.
            if (isInteractable)
            {
                if (GameManager.Instance.data.dialog.TryGetValue(4, out var dialogWrapper)) // 4�� ���̾�α� ����
                {
                    LookAt(Player.Current.transform.position);

                    // ���̾�α� ����.
                    GameManager.Instance.uiManager.dialog.StartDialog(dialogWrapper.DialogTextList, () =>
                    {
                        playerTr = Player.Current.transform;
                        isInteractable = false;
                        this.gameObject.layer = LayerMask.NameToLayer(GameManager.MONSTER_LAYER_NAME);

                        rb.bodyType = RigidbodyType2D.Dynamic;

                        interactionCallback?.Invoke();
                    });

                    overheadUI.ActiveG_Key(false);
                }
            }
        }

        private void LookAt(Vector3 targetPos)
        {
            Vector3 dir = targetPos - this.transform.position;

            if (dir.x > 0f)
            {
                this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                this.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        #region Behaviour Tree

        [ContextMenu("BT �ʱ�ȭ �Լ�")]
        private void ResetBehaviourTree()
        {
            currentSkill = SkillState.None;
            currentPlayingAnim = -1;
            phase = 1;
            nextActionTime = -1f;
            hp = info.hp;

            anim.Play(AnimHash.IDLE);
        }

        // ���� Behaviour Tree ��ȯ
        private IBehaviourTreeNode GetBehaviourTree()
        {
            var builder = new BehaviourTreeBuilder();

            // BT..
            builder.Selector(string.Empty)
                .Sequence(string.Empty) // ������ üũ
                    .Condition(string.Empty, t => hp <= 0) // ü���� 0���� �۰ų� ������ ������ ���� �õ�
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
                            if (currentSkill == SkillState.None)
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
                    LookAt(playerTr.position);

                    anim.Play(AnimHash.IDLE);

                    return BehaviourTreeStatus.Success;
                })
            .End();

            return builder.Build();
        }

        /// <summary>
        /// ������ ��ų�� ��ȯ�ϴ� �Լ�
        /// </summary>
        private SkillState GetSkillRandomly()
        {
            System.Random random = new System.Random();

            int value = random.Next(0, 4);
            SkillState result = SkillState.None;

            switch (value)
            {
                case 0:
                {
                    result = SkillState.Slash;
                    break;
                }
                case 1:
                {
                    result = SkillState.EjectSlash;
                    break;
                }
                case 2:
                {
                    result = SkillState.Explosion;
                    break;
                }
                case 3:
                {
                    result = SkillState.Rush;
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
                .Do(string.Empty, t =>
                {
                    if (currentSkill == SkillState.Slash)
                        return BehaviourTreeStatus.Success;
                    else
                        return BehaviourTreeStatus.Failure;
                })
                .Do(string.Empty, t => // �ִϸ����Ϳ� ��ũ��Ʈ ���� ��ũ �׽�Ʈ
                {
                    if (currentPlayingAnim != AnimHash.SLASH) // Play �޼��带 ȣ���Ϸ��� ����.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.SLASH) // Play �޼��尡 ȣ�� �Ǿ���, ����ȭ�� �̷������ ���� ����.
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else  // Play �޼��尡 ȣ��Ǿ���, �ִϸ����Ϳ� ��ũ�� ��ġ�ϴ� ����.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                })
                .Selector(string.Empty)
                    .Do(string.Empty, t =>
                    {
                        if (currentPlayingAnim != AnimHash.SLASH) // ���� ������� �ִϸ��̼��� ���İ� �ƴ� ���
                            return BehaviourTreeStatus.Failure;

                        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // ���� �ִϸ��̼��� ���� ���
                        {
                            currentSkill = SkillState.None;
                            nextActionTime = Time.time + afterDelay;

                            anim.Play(AnimHash.IDLE);
                            currentPlayingAnim = AnimHash.IDLE;

                            return BehaviourTreeStatus.Success;
                        }
                        else
                        {
                            return BehaviourTreeStatus.Running;
                        }
                    })

                    .Selector(string.Empty)
                        .Do(string.Empty, t => // �ִϸ��̼��� ����.
                        {
                            if (Mathf.Abs(playerTr.position.x - transform.position.x) < xDistance) // �����Ÿ� ����
                            {
                                anim.Play(AnimHash.SLASH);
                                currentPlayingAnim = AnimHash.SLASH;

                                if (slashSkill == null)
                                {
                                    slashSkill = new Skill(slashSkillData, 0);
                                }

                                if (slashSkill.TryOperate(transform.position, transform.rotation, LayerMask.NameToLayer(GameManager.MONSTER_EXCLUSIVE_LAYER_NAME), this))
                                {
                                    return BehaviourTreeStatus.Running;
                                }
                                else
                                {
                                    return BehaviourTreeStatus.Failure;
                                }
                            }
                            else
                            {
                                return BehaviourTreeStatus.Failure;
                            }
                        })

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

        public float speed = 5f;

        // ���� ��ų ���� Ʈ�� ��ȯ
        private IBehaviourTreeNode GetEjectSlashBehaviourTree()
        {
            float afterDelay = 2f;
            float xDistance = 60f;

            BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

            builder.Sequence(string.Empty)
                .Do(string.Empty, t =>
                {
                    if (currentSkill == SkillState.EjectSlash)
                        return BehaviourTreeStatus.Success;
                    else
                        return BehaviourTreeStatus.Failure;
                })
                .Do(string.Empty, t => // �ִϸ����Ϳ� ��ũ��Ʈ ���� ��ũ �׽�Ʈ
                {
                    if (currentPlayingAnim != AnimHash.EJECT_SLASH) // Play �޼��带 ȣ���Ϸ��� ����.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.EJECT_SLASH) // Play �޼��尡 ȣ�� �Ǿ���, ����ȭ�� �̷������ ���� ����.
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else  // Play �޼��尡 ȣ��Ǿ���, �ִϸ����Ϳ� ��ũ�� ��ġ�ϴ� ����.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                })
                .Selector(string.Empty)
                    .Do(string.Empty, t =>
                    {
                        if (currentPlayingAnim != AnimHash.EJECT_SLASH) // ���� ������� �ִϸ��̼��� ���İ� �ƴ� ���
                            return BehaviourTreeStatus.Failure;

                        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // ���� �ִϸ��̼��� ���� ���
                        {
                            currentSkill = SkillState.None;
                            nextActionTime = Time.time + afterDelay;

                            anim.Play(AnimHash.IDLE);
                            currentPlayingAnim = AnimHash.IDLE;

                            return BehaviourTreeStatus.Success;
                        }
                        else
                        {
                            return BehaviourTreeStatus.Running;
                        }
                    })

                    .Selector(string.Empty)
                        .Do(string.Empty, t => // �ִϸ��̼��� ����.
                        {
                            if (Mathf.Abs(playerTr.position.x - transform.position.x) < xDistance) // �����Ÿ� ����
                            {
                                anim.Play(AnimHash.EJECT_SLASH);
                                currentPlayingAnim = AnimHash.EJECT_SLASH;

                                Vector2 dir = new Vector2((playerTr.position - transform.position).x, 0f);

                                if (ejectSlashSkill == null)
                                {
                                    ejectSlashSkill = new Skill(ejectSlashSkillData, 0);
                                }

                                if (ejectSlashSkill.TryOperateLinearDynamic(transform.position, transform.rotation, LayerMask.NameToLayer(GameManager.MONSTER_EXCLUSIVE_LAYER_NAME), this, dir, 2f))
                                {
                                    return BehaviourTreeStatus.Running;
                                }
                                else
                                {
                                    return BehaviourTreeStatus.Failure;
                                }
                            }
                            else
                            {
                                return BehaviourTreeStatus.Failure;
                            }
                        })

                        .Do(string.Empty, t => // �÷��̾������� �̵�
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            LookAt(playerTr.position);

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
                .Do(string.Empty, t =>
                {
                    if (currentSkill == SkillState.Explosion)
                        return BehaviourTreeStatus.Success;
                    else
                        return BehaviourTreeStatus.Failure;
                })
                .Do(string.Empty, t => // �ִϸ����Ϳ� ��ũ��Ʈ ���� ��ũ �׽�Ʈ
                {
                    if (currentPlayingAnim != AnimHash.EXPLOSION) // Play �޼��带 ȣ���Ϸ��� ����.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.EXPLOSION) // Play �޼��尡 ȣ�� �Ǿ���, ����ȭ�� �̷������ ���� ����.
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else  // Play �޼��尡 ȣ��Ǿ���, �ִϸ����Ϳ� ��ũ�� ��ġ�ϴ� ����.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                })
                .Selector(string.Empty)
                    .Do(string.Empty, t =>
                    {
                        if (currentPlayingAnim != AnimHash.EXPLOSION) // ���� ������� �ִϸ��̼��� ���İ� �ƴ� ���
                            return BehaviourTreeStatus.Failure;

                        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // ���� �ִϸ��̼��� ���� ���
                        {
                            currentSkill = SkillState.None;
                            nextActionTime = Time.time + afterDelay;

                            anim.Play(AnimHash.IDLE);
                            currentPlayingAnim = AnimHash.IDLE;

                            return BehaviourTreeStatus.Success;
                        }
                        else
                        {
                            return BehaviourTreeStatus.Running;
                        }
                    })

                    .Selector(string.Empty)
                        .Do(string.Empty, t => // �ִϸ��̼��� ����.
                        {
                            if (Mathf.Abs(playerTr.position.x - transform.position.x) < xDistance) // �����Ÿ� ����
                            {
                                anim.Play(AnimHash.EXPLOSION);
                                currentPlayingAnim = AnimHash.EXPLOSION;

                                if (explosionSkill == null)
                                {
                                    explosionSkill = new Skill(explosionSkillData, 0);
                                }

                                if (explosionSkill.TryOperate(transform.position, transform.rotation, LayerMask.NameToLayer(GameManager.MONSTER_EXCLUSIVE_LAYER_NAME), this))
                                {
                                    return BehaviourTreeStatus.Running;
                                }
                                else
                                {
                                    return BehaviourTreeStatus.Failure;
                                }
                            }
                            else
                            {
                                return BehaviourTreeStatus.Failure;
                            }
                        })

                        .Do(string.Empty, t => // �÷��̾������� �̵�
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            LookAt(playerTr.position);

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
        public float rushSpeed = 4f;
        // ���� ��ų ���� Ʈ�� ��ȯ
        private IBehaviourTreeNode GetRushBehaviourTree()
        {
            float afterDelay = 2f;
            float xDistance = 60f;

            BehaviourTreeBuilder builder = new BehaviourTreeBuilder();
            Vector2 rushDir = Vector2.zero;

            builder.Sequence(string.Empty)
                .Do(string.Empty, t =>
                {
                    if (currentSkill == SkillState.Rush)
                        return BehaviourTreeStatus.Success;
                    else
                        return BehaviourTreeStatus.Failure;
                })
                .Do(string.Empty, t => // �ִϸ����Ϳ� ��ũ��Ʈ ���� ��ũ �׽�Ʈ
                {
                    if (currentPlayingAnim != AnimHash.RUSH) // Play �޼��带 ȣ���Ϸ��� ����.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.RUSH) // Play �޼��尡 ȣ�� �Ǿ���, ����ȭ�� �̷������ ���� ����.
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else  // Play �޼��尡 ȣ��Ǿ���, �ִϸ����Ϳ� ��ũ�� ��ġ�ϴ� ����.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                })
                .Selector(string.Empty)
                    .Do(string.Empty, t =>
                    {
                        if (currentPlayingAnim != AnimHash.RUSH) // ���� ������� �ִϸ��̼��� ������ �ƴ� ���
                            return BehaviourTreeStatus.Failure;

                        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // ���� �ִϸ��̼��� ���� ���
                        {
                            currentSkill = SkillState.None;
                            nextActionTime = Time.time + afterDelay;

                            anim.Play(AnimHash.IDLE);
                            currentPlayingAnim = AnimHash.IDLE;

                            rushDir = Vector2.zero; // ���� �ܺ� ���� ���� �ʱ�ȭ

                            return BehaviourTreeStatus.Success;
                        }
                        else
                        {
                            rb.linearVelocity = rushDir * rushSpeed;

                            return BehaviourTreeStatus.Running;
                        }
                    })

                    .Selector(string.Empty)
                        .Do(string.Empty, t => // �ִϸ��̼��� ����.
                        {
                            if (Mathf.Abs(playerTr.position.x - transform.position.x) < xDistance) // �����Ÿ� ����
                            {
                                if (rushSkill == null)
                                {
                                    rushSkill = new Skill(rushSkillData, 0);
                                }

                                if (rushSkill.TryOperate(transform.position, transform.rotation, LayerMask.NameToLayer(GameManager.MONSTER_EXCLUSIVE_LAYER_NAME), this))
                                {
                                    anim.Play(AnimHash.RUSH);
                                    currentPlayingAnim = AnimHash.RUSH;

                                    rushDir = (playerTr.position - transform.position) * Vector2.right; // �÷��̾ ���� x�� ���⺤�� ����
                                    rushDir.Normalize();

                                    return BehaviourTreeStatus.Running;
                                }
                                else
                                {
                                    return BehaviourTreeStatus.Failure;
                                }
                            }
                            else
                            {
                                return BehaviourTreeStatus.Failure;
                            }
                        })

                        .Do(string.Empty, t => // �÷��̾������� �̵�
                        {
                            Vector3 dir = playerTr.position - transform.position;

                            LookAt(playerTr.position);

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