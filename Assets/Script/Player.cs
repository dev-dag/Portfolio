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
    /// ���� ���� ��ȯ
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
            .Selector("�÷��̾� BT")
                .Condition(string.Empty, (t) => playerInfo.isDead)

                .Sequence(string.Empty)
                    .Condition("���� ���", (t) => playerInfo.HP <= 0f)
                    .Do(string.Empty, DoOnDead)
                .End()

                .Sequence(string.Empty)
                    .Do("��/�� �̵� �Է� ó��", DoDefaultInputProc)
                    .Selector(string.Empty)
                        .Selector(string.Empty)
                            .Selector("���� �Է� ó��")
                                .Sequence(string.Empty)
                                    .Condition("���� ���� ���", (t) => anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
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
                                    .Condition("���� ��ư�� ���� ���", (t) => attackAction.IsPressed())
                                    .Do(string.Empty, DoOnAttack)
                                .End()
                            .End()

                            .Selector("���� �� �߶� ó��")
                                .Sequence(string.Empty)
                                    .Condition("���� ��ư�� ���� ���", (t) => jumpAction.IsPressed() && jumpAction.IsInProgress())
                                    .Sequence(string.Empty)
                                        .Condition(string.Empty, (t) => RB.linearVelocityY.IsAlmostEqaul(0f))
                                        .Do(string.Empty, DoOnJump)
                                    .End()
                                .End()

                                .Sequence(string.Empty)
                                    .Condition("�߶� ���� ���", (t) => RB.linearVelocityY < -0.1f)
                                    .Do(string.Empty, DoOnFall)
                                .End()

                                .Condition("���� ���� ���", (t) => anim.GetCurrentAnimatorStateInfo(0).shortNameHash == AnimHash.JUMP)
                            .End()

                            .Do("Run/Idle �ִϸ��̼�", DoOnIdleAndRun)
                        .End()
                    .End()
            .End();

            return builder.Build();
    }

    private BehaviourTreeStatus DoOnDead(TimeData t)
    {
        // ��� �ִϸ��̼� ���
        anim.Play(AnimHash.DEAD);

        // ���� ���� �ʵ� ����
        playerInfo.isDead = true;

        return BehaviourTreeStatus.Success;
    }

    private BehaviourTreeStatus DoOnFall(TimeData t)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.FALL)
        {
            // �߶� �ִϸ��̼� ���
            anim.Play(AnimHash.FALL);
        }

        return BehaviourTreeStatus.Running;
    }

    private BehaviourTreeStatus DoOnJump(TimeData t)
    {
        // ���� �ִϸ��̼� ���
        anim.Play(AnimHash.JUMP);

        RB.linearVelocityY += playerInfo.jumpPower;

        return BehaviourTreeStatus.Running;
    }

    private BehaviourTreeStatus DoOnAttack(TimeData t)
    {
        // ���� �ִϸ��̼� ���
        anim.Play(AnimHash.ATTACK_1);

        return BehaviourTreeStatus.Running;
    }

    private BehaviourTreeStatus DoOnIdleAndRun(TimeData t)
    {
        if (RB.linearVelocityX.Abs() < 0.05f) 
        {
            // Velocity X�� ������ 0.05���� ���� ��� Idle �ִϸ��̼� ���
            anim.Play(AnimHash.IDLE);
        }
        else 
        {
            // Run �ִϸ��̼� ���
            anim.Play(AnimHash.RUN);
        }

        return BehaviourTreeStatus.Success;
    }

    /// <summary>
    /// �⺻ �Է� ó��
    /// </summary>
    private BehaviourTreeStatus DoDefaultInputProc(TimeData t)
    {
        if (moveAction.IsPressed() && moveAction.IsInProgress())
        {
            Vector2 dir = moveAction.ReadValue<Vector2>();
            float newX = dir.x;

            RB.linearVelocityX = newX * playerInfo.speed;

            // ȸ�� ó��
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
