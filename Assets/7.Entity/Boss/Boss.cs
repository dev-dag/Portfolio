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
        }

        public bool IsDead { get; private set; } = false;

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
        private float nextActionTime; // 스킬의 후딜레이를 지정하기 위해 사용함. 게임의 시간 값이 할당 됨.
        private SkillState currentSkill = SkillState.None;
        private int currentPlayingAnim;
        private bool isInteractable = true;
        private Awaitable colorFadeAwaiter = null;
        private float colorFadeTime = 0.2f;

        private Skill slashSkill;
        private Skill ejectSlashSkill;
        private Skill explosionSkill;
        private Skill rushSkill;

        private AudioPlayer audioPlayer;

        private void Awake()
        {
            Init();

            this.gameObject.layer = LayerMask.NameToLayer(GameManager.INTERACTABLE_OBJECT_LAYER_NAME);
            if (this.gameObject.layer == -1)
            {
                EDebug.LogError("레이어 이름 오류");
            }

            bt = GetBehaviourTree();
        }

        private void Start()
        {
            overheadUI = GameManager.Instance.gameUI.OverheadUI_Pool.Burrow<OverheadUI>();
            overheadUI.Init(this.transform, Vector3.zero);
            overheadUI.Enable();

            rb.bodyType = RigidbodyType2D.Kinematic; // 첫 조우 시 상호작용 전에 원 위치에서 이탈 방지

            audioPlayer = GameManager.Instance.audioSystem.GetUnManagedAudioPlayer(AudioSystem.AudioType.SFX);
        }

        private void Update()
        {
            if (playerTr != null)
            {
                bt.Tick(new TimeData(Time.deltaTime));
            }
        }

        void ICombatable.TakeHit(int damage, Entity hitter)
        {
            if (isInteractable || IsDead)
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
            // IsInteractable == True 일 때만 호출 가능.
            // 대화 취소했을 때 처리. 다시 대화 가능하게 설정.
            isInteractable = true;
        }

        bool IInteractable.IsInteractable()
        {
            // 처음 조우 했을 때 한번 대화 가능.
            return isInteractable;
        }

        void IInteractable.SetInteractionGuide(bool isActive)
        {
            // 상호작용 UI 노출
            overheadUI.ActiveG_Key(isActive);
        }

        void IInteractable.StartInteraction(Action interactionCallback)
        {
            // IsInteractable == true 일 때만 호출 가능.
            if (isInteractable)
            {
                if (GameManager.Instance.ReferenceData.dialog.TryGetValue(4, out var dialogWrapper)) // 4번 다이얼로그 시작
                {
                    LookAt(Player.Current.transform.position);

                    // 다이얼로그 시작.
                    GameManager.Instance.gameUI.Dialog.StartDialog(dialogWrapper.DialogTextList, () =>
                    {
                        isInteractable = false;
                        this.gameObject.layer = LayerMask.NameToLayer(GameManager.MONSTER_LAYER_NAME);
                        OnInteractionEnd();
                        rb.bodyType = RigidbodyType2D.Dynamic;

                        interactionCallback?.Invoke();
                    });

                    overheadUI.ActiveG_Key(false);
                }
            }
        }

        private async Awaitable OnInteractionEnd()
        {
            await Awaitable.WaitForSecondsAsync(1f);

            playerTr = Player.Current.transform;
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

        [ContextMenu("BT 초기화 함수")]
        private void ResetBehaviourTree()
        {
            currentSkill = SkillState.None;
            currentPlayingAnim = -1;
            nextActionTime = -1f;
            hp = info.hp;
            IsDead = false;

            anim.Play(AnimHash.IDLE);
        }

        // 메인 Behaviour Tree 반환
        private IBehaviourTreeNode GetBehaviourTree()
        {
            var builder = new BehaviourTreeBuilder();

            // BT..
            builder.Selector(string.Empty)
                .Sequence(string.Empty)
                    .Condition(string.Empty, (t) => IsDead)
                    .Do(string.Empty, (t) =>
                    {
                        if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash == AnimHash.DIE
                            && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                        {
                            Time.timeScale = 1f;
                        }

                        return BehaviourTreeStatus.Success;
                    })
                .End()

                .Sequence(string.Empty) // 사망 체크
                    .Condition(string.Empty, t => hp <= 0) // 체력이 0보다 작거나 같으면 사망처리
                    .Sequence(string.Empty)
                        .Do(string.Empty, t =>
                        {
                            if (IsDead == false)
                            {
                                IsDead = true;
                                anim.Play(AnimHash.DIE);
                                currentPlayingAnim = AnimHash.DIE;

                                Time.timeScale = 0.2f;

                                (LevelControl.Current as CastleOfTheKingLevelControl).DungeonClear();
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

                .Do(string.Empty, t => // 플레이어를 바라보는 동작 처리.
                {
                    LookAt(playerTr.position);

                    anim.Play(AnimHash.IDLE);

                    return BehaviourTreeStatus.Success;
                })
            .End();

            return builder.Build();
        }

        /// <summary>
        /// 랜덤한 스킬을 반환하는 함수
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

            return result; // None이면 안됌.
        }

        // 공격 스킬 서브 트리 반환
        private IBehaviourTreeNode GetSlashBehaviourTree()
        {
            float afterDelay = 2f;
            float xDistance = 7.5f;

            BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

            builder.Sequence(string.Empty)
                .Do(string.Empty, t =>
                {
                    if (currentSkill == SkillState.Slash)
                        return BehaviourTreeStatus.Success;
                    else
                        return BehaviourTreeStatus.Failure;
                })
                .Do(string.Empty, t => // 애니메이터와 스크립트 간의 싱크 테스트
                {
                    if (currentPlayingAnim != AnimHash.SLASH) // Play 메서드를 호출하려는 시점.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.SLASH) // Play 메서드가 호출 되었고, 동기화가 이루어지지 않은 시점.
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else  // Play 메서드가 호출되었고, 애니메이터와 싱크가 일치하는 시점.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                })
                .Selector(string.Empty)
                    .Do(string.Empty, t =>
                    {
                        if (currentPlayingAnim != AnimHash.SLASH) // 현재 재생중인 애니메이션이 폭파가 아닌 경우
                            return BehaviourTreeStatus.Failure;

                        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // 폭파 애니메이션이 끝난 경우
                        {
                            currentSkill = SkillState.None;
                            nextActionTime = Time.time + afterDelay;

                            audioPlayer.Stop(); // SFX 정지

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
                        .Do(string.Empty, t => // 애니메이션의 시점.
                        {
                            if (Mathf.Abs(playerTr.position.x - transform.position.x) < xDistance) // 사정거리 조건
                            {
                                audioPlayer.Play(slashSkillData.SFX_Clips[0]); // SFX 재생

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

        public float speed = 5f;

        // 공격 스킬 서브 트리 반환
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
                .Do(string.Empty, t => // 애니메이터와 스크립트 간의 싱크 테스트
                {
                    if (currentPlayingAnim != AnimHash.EJECT_SLASH) // Play 메서드를 호출하려는 시점.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.EJECT_SLASH) // Play 메서드가 호출 되었고, 동기화가 이루어지지 않은 시점.
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else  // Play 메서드가 호출되었고, 애니메이터와 싱크가 일치하는 시점.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                })
                .Selector(string.Empty)
                    .Do(string.Empty, t =>
                    {
                        if (currentPlayingAnim != AnimHash.EJECT_SLASH) // 현재 재생중인 애니메이션이 폭파가 아닌 경우
                            return BehaviourTreeStatus.Failure;

                        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // 폭파 애니메이션이 끝난 경우
                        {
                            currentSkill = SkillState.None;
                            nextActionTime = Time.time + afterDelay;

                            audioPlayer.Stop(); // SFX 정지

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
                        .Do(string.Empty, t => // 애니메이션의 시점.
                        {
                            if (Mathf.Abs(playerTr.position.x - transform.position.x) < xDistance) // 사정거리 조건
                            {
                                audioPlayer.Play(ejectSlashSkillData.SFX_Clips[0]); // SFX 재생

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

                        .Do(string.Empty, t => // 플레이어쪽으로 이동
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
        
        // 공격 스킬 서브 트리 반환
        private IBehaviourTreeNode GetExplosionBehaviourTree()
        {
            float afterDelay = 2f;
            float xDistance = 7f;

            BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

            builder.Sequence(string.Empty)
                .Do(string.Empty, t =>
                {
                    if (currentSkill == SkillState.Explosion)
                        return BehaviourTreeStatus.Success;
                    else
                        return BehaviourTreeStatus.Failure;
                })
                .Do(string.Empty, t => // 애니메이터와 스크립트 간의 싱크 테스트
                {
                    if (currentPlayingAnim != AnimHash.EXPLOSION) // Play 메서드를 호출하려는 시점.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.EXPLOSION) // Play 메서드가 호출 되었고, 동기화가 이루어지지 않은 시점.
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else  // Play 메서드가 호출되었고, 애니메이터와 싱크가 일치하는 시점.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                })
                .Selector(string.Empty)
                    .Do(string.Empty, t =>
                    {
                        if (currentPlayingAnim != AnimHash.EXPLOSION) // 현재 재생중인 애니메이션이 폭파가 아닌 경우
                            return BehaviourTreeStatus.Failure;

                        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // 폭파 애니메이션이 끝난 경우
                        {
                            currentSkill = SkillState.None;
                            nextActionTime = Time.time + afterDelay;

                            audioPlayer.Stop(); // SFX 정지

                            anim.Play(AnimHash.IDLE);
                            currentPlayingAnim = AnimHash.IDLE;

                            return BehaviourTreeStatus.Success;
                        }
                        else if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
                        {
                            if (audioPlayer.GetCurrentClip() != explosionSkillData.SFX_Clips[1])
                            {
                                audioPlayer.Play(explosionSkillData.SFX_Clips[1]); // SFX 재생
                            }

                            return BehaviourTreeStatus.Running;
                        }
                        else
                        {
                            return BehaviourTreeStatus.Running;
                        }
                    })

                    .Selector(string.Empty)
                        .Do(string.Empty, t => // 애니메이션의 시점.
                        {
                            if (Mathf.Abs(playerTr.position.x - transform.position.x) < xDistance) // 사정거리 조건
                            {
                                audioPlayer.Play(explosionSkillData.SFX_Clips[0]); // SFX 재생

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

                        .Do(string.Empty, t => // 플레이어쪽으로 이동
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
        // 공격 스킬 서브 트리 반환
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
                .Do(string.Empty, t => // 애니메이터와 스크립트 간의 싱크 테스트
                {
                    if (currentPlayingAnim != AnimHash.RUSH) // Play 메서드를 호출하려는 시점.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.RUSH) // Play 메서드가 호출 되었고, 동기화가 이루어지지 않은 시점.
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else  // Play 메서드가 호출되었고, 애니메이터와 싱크가 일치하는 시점.
                    {
                        return BehaviourTreeStatus.Success;
                    }
                })
                .Selector(string.Empty)
                    .Do(string.Empty, t =>
                    {
                        if (currentPlayingAnim != AnimHash.RUSH) // 현재 재생중인 애니메이션이 돌진이 아닌 경우
                            return BehaviourTreeStatus.Failure;

                        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // 돌진 애니메이션이 끝난 경우
                        {
                            currentSkill = SkillState.None;
                            nextActionTime = Time.time + afterDelay;

                            audioPlayer.Stop(); // SFX 정지

                            anim.Play(AnimHash.IDLE);
                            currentPlayingAnim = AnimHash.IDLE;

                            rushDir = Vector2.zero; // 람다 외부 변수 수동 초기화

                            return BehaviourTreeStatus.Success;
                        }
                        else
                        {
                            rb.linearVelocity = rushDir * rushSpeed;

                            return BehaviourTreeStatus.Running;
                        }
                    })

                    .Selector(string.Empty)
                        .Do(string.Empty, t => // 애니메이션의 시점.
                        {
                            if (Mathf.Abs(playerTr.position.x - transform.position.x) < xDistance) // 사정거리 조건
                            {
                                if (rushSkill == null)
                                {
                                    rushSkill = new Skill(rushSkillData, 0);
                                }

                                if (rushSkill.TryOperateWithFollow(transform, transform.rotation, LayerMask.NameToLayer(GameManager.MONSTER_EXCLUSIVE_LAYER_NAME), this))
                                {
                                    audioPlayer.Play(rushSkillData.SFX_Clips[0]); // SFX 재생

                                    anim.Play(AnimHash.RUSH);
                                    currentPlayingAnim = AnimHash.RUSH;

                                    rushDir = (playerTr.position - transform.position) * Vector2.right; // 플레이어를 향한 x축 방향벡터 산출
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

                        .Do(string.Empty, t => // 플레이어쪽으로 이동
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