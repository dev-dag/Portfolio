using UnityEngine;
using FluentBehaviourTree;
using UnityEngine.InputSystem;
using System.Timers;

public class Player : BaseObject
{
    public struct AnimHash
    {
        public static readonly int IDLE = Animator.StringToHash("Idle");
        public static readonly int RUN = Animator.StringToHash("Run");
        public static readonly int ATTACK_1 = Animator.StringToHash("PunchA");
        public static readonly int JUMP = Animator.StringToHash("Jump");
        public static readonly int FALL = Animator.StringToHash("JumpFall");
        public static readonly int DEAD = Animator.StringToHash("Die");
    }

    public static Player Current { get; private set; }

    [SerializeField] private InputActionAsset inputAction;

    [Space(20f)]
    [SerializeField] private PlayerInfo playerInfo;
    [SerializeField] private Rigidbody2D RB;
    [SerializeField] private Animator anim;

    private IBehaviourTreeNode BT_Root;

    private InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    protected override void Awake()
    {
        base.Awake();

        if (Current == null)
        {
            Current = this;
        }

        ActiveInput();
    }

    protected override void Start()
    {
        base.Start();

        InitStatus();
    }

    protected override void Update()
    {
        base.Update();

        BT_Root.Tick(new TimeData(Time.deltaTime));
    }

    private void OnDestroy()
    {
        Current = null;
    }

    public void InitStatus()
    {
        playerInfo.Init();
    }

    /// <summary>
    /// 생존 여부 반환
    /// </summary>
    public bool IsAlive()
    {
        return playerInfo.HP > 0f ? false : true;
    }

    private void ActiveInput()
    {
        inputAction.Enable();

        actionMap = inputAction.FindActionMap("Player");

        moveAction = actionMap.FindAction("Move");
        jumpAction = actionMap.FindAction("Jump");
        attackAction = actionMap.FindAction("Attack");

        BT_Root = MakeBehaviourTree();
    }

    private IBehaviourTreeNode MakeBehaviourTree()
    {
        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

        builder = builder
            .Selector("플레이어 BT")
                .Condition(string.Empty, (t) => playerInfo.isDead)

                .Sequence(string.Empty)
                    .Condition("죽은 경우", (t) => playerInfo.HP <= 0f)
                    .Do(string.Empty, DoOnDead)
                .End()

                .Sequence(string.Empty)
                    .Do("좌/우 이동 입력 처리", DoDefaultInputProc)
                    .Selector(string.Empty)
                        .Selector(string.Empty)
                            .Selector("공격 입력 처리")
                                .Sequence(string.Empty)
                                    .Condition("공격 중인 경우", (t) => anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                                    .Do(string.Empty, (t) =>
                                        {
                                            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                                            {
                                                return BehaviourTreeStatus.Failure;
                                            }
                                            else
                                            {
                                                return BehaviourTreeStatus.Running;
                                            }
                                        })
                                .End()

                                .Sequence(string.Empty)
                                    .Condition("공격 버튼을 누른 경우", (t) => attackAction.IsPressed())
                                    .Do(string.Empty, DoOnAttack)
                                .End()
                            .End()

                            .Selector("점프 및 추락 처리")
                                .Sequence(string.Empty)
                                    .Condition("점프 버튼이 눌린 경우", (t) => jumpAction.IsPressed() && jumpAction.IsInProgress())
                                    .Sequence(string.Empty)
                                        .Condition(string.Empty, (t) => RB.linearVelocityY.IsAlmostEqaul(0f))
                                        .Do(string.Empty, DoOnJump)
                                    .End()
                                .End()

                                .Sequence(string.Empty)
                                    .Condition("추락 중인 경우", (t) => RB.linearVelocityY < -0.1f)
                                    .Do(string.Empty, DoOnFall)
                                .End()

                                .Condition("점프 중인 경우", (t) => anim.GetCurrentAnimatorStateInfo(0).shortNameHash == AnimHash.JUMP)
                            .End()

                            .Do("Run/Idle 애니메이션", DoOnIdleAndRun)
                        .End()
                    .End()
            .End();

            return builder.Build();
    }

    private BehaviourTreeStatus DoOnDead(TimeData t)
    {
        // 사망 애니메이션 재생
        anim.Play(AnimHash.DEAD);

        // 생존 여부 필드 설정
        playerInfo.isDead = true;

        return BehaviourTreeStatus.Success;
    }

    private BehaviourTreeStatus DoOnFall(TimeData t)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.FALL)
        {
            // 추락 애니메이션 재생
            anim.Play(AnimHash.FALL);
        }

        return BehaviourTreeStatus.Running;
    }

    private BehaviourTreeStatus DoOnJump(TimeData t)
    {
        // 점프 애니메이션 재생
        anim.Play(AnimHash.JUMP);

        RB.linearVelocityY += playerInfo.jumpPower;

        return BehaviourTreeStatus.Running;
    }

    private BehaviourTreeStatus DoOnAttack(TimeData t)
    {
        // 공격 애니메이션 재생
        anim.Play(AnimHash.ATTACK_1);

        return BehaviourTreeStatus.Running;
    }

    private BehaviourTreeStatus DoOnIdleAndRun(TimeData t)
    {
        if (RB.linearVelocityX.Abs() < 0.05f) 
        {
            // Velocity X의 절댓값이 0.05보다 작은 경우 Idle 애니메이션 재생
            anim.Play(AnimHash.IDLE);
        }
        else 
        {
            // Run 애니메이션 재생
            anim.Play(AnimHash.RUN);
        }

        return BehaviourTreeStatus.Success;
    }

    /// <summary>
    /// 기본 입력 처리
    /// </summary>
    private BehaviourTreeStatus DoDefaultInputProc(TimeData t)
    {
        if (moveAction.IsPressed() && moveAction.IsInProgress())
        {
            Vector2 dir = moveAction.ReadValue<Vector2>();
            float newX = dir.x;

            RB.linearVelocityX = newX * playerInfo.speed;

            // 회전 처리
            {
                float newRotY;

                if (dir.x < 0f)
                {
                    newRotY = 180f;
                }
                else
                {
                    newRotY = 0f;
                }

                RB.transform.rotation = Quaternion.Euler(RB.transform.rotation.x, newRotY, RB.transform.rotation.z);
            }
        }

        return BehaviourTreeStatus.Success;
    }
}
