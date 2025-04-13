using UnityEngine;
using FluentBehaviourTree;
using UnityEngine.InputSystem;
using System.Timers;
using Unity.Cinemachine;
using UnityEngine.Rendering;

public class Player : BaseObject, ICombatable
{
    public struct AnimHash
    {
        public static readonly int IDLE = Animator.StringToHash("Idle");
        public static readonly int RUN = Animator.StringToHash("Run");
        public static readonly int ATTACK_1 = Animator.StringToHash("SwordAttack");
        public static readonly int JUMP = Animator.StringToHash("Jump");
        public static readonly int FALL = Animator.StringToHash("JumpFall");
        public static readonly int DEAD = Animator.StringToHash("Die");
    }

    public static Player Current { get; private set; }

    public SkillData testSkillData;

    [Space(20f)]
    [SerializeField] private PlayerInfo playerInfo;
    [SerializeField] private Rigidbody2D RB;
    [SerializeField] private Animator anim;

    [Space(20f)]
    [SerializeField] private float interactableDistance = 2f;

    private IBehaviourTreeNode BT_Root;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction interactAction;

    private IInteractable interactionCurrent; // ���� ���ͷ��� ������ ���
    private bool onInteration = false;

    protected override void Awake()
    {
        base.Awake();

        Current = this;

        Init();

        AttachCamera();
    }

    protected override void Start()
    {
        base.Start();
    }
    int f = 1;
    protected override void Update()
    {
        base.Update();

        BT_Root.Tick(new TimeData(Time.deltaTime));

        CheckInteraction();
    }

    private void OnDestroy()
    {
        if (Current == this)
        {
            Current = null;
        }
    }

    private void OnEnable()
    {
        interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteract;
    }

    /// <summary>
    /// ���� ���� ��ȯ
    /// </summary>
    public bool IsAlive()
    {
        return playerInfo.HP > 0f ? false : true;
    }

    /// <summary>
    /// ��ȣ�ۿ� UI ��� ���� ���� üũ
    /// </summary>
    private void CheckInteraction()
    {
        if (onInteration) // ��ȣ�ۿ� ���ΰ�� ��ȯ
        {
            return;
        }

        RaycastHit2D[] hitArr = Physics2D.BoxCastAll(transform.position, Vector2.one * interactableDistance, 0f, Vector2.up, 0f, LayerMask.GetMask(GameManager.INTERACTABLE_OBJECT_LAYER_NAME));
        if (hitArr == null || hitArr.Length == 0)
        {
            if (interactionCurrent != null)
            {
                interactionCurrent.SetInteractionGuide(false);
                interactionCurrent = null;
            }

            return;
        }

        foreach (RaycastHit2D hit in hitArr)
        {
            IInteractable interactableHit = hit.collider.GetComponent<IInteractable>();

            if (interactableHit.IsInteractable() == false) // ��ȣ�ۿ� �Ұ����� ��� ����
            {
                continue;
            }

            if (interactionCurrent == null)
            {
                interactionCurrent = interactableHit;

                interactionCurrent.SetInteractionGuide(true);
            }
            else if (interactableHit != interactionCurrent) // ���� ��ȣ�ۿ� ������ ��� ���� �� ��ȯ
            {
                interactionCurrent.SetInteractionGuide(false);
                interactionCurrent = interactableHit;

                interactionCurrent.SetInteractionGuide(true);

                return;
            }
        }
    }

    private void AttachCamera()
    {
        var cam = GameObject.FindWithTag("MainCamera");
        var cineCam = cam.GetComponent<CinemachineCamera>();
        cineCam.Target = new CameraTarget()
        {
            TrackingTarget = this.transform
        };
    }

    private void Init()
    {
        InputActionMap actionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("Player");

        moveAction = actionMap.FindAction("Move");
        jumpAction = actionMap.FindAction("Jump");
        attackAction = actionMap.FindAction("Attack");

        // UI �Է� �ʱ�ȭ
        InputActionMap UI_ActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("UI");
        interactAction = UI_ActionMap.FindAction("Interact");

        playerInfo.Init();

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
                                    .Condition("���� ��ư�� ���� ���", (t) => attackAction.IsPressed() && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") == false)
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
                .End()

                .Do("Run/Idle �ִϸ��̼�", DoOnIdleAndRun)
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

    Skill skill;

    private BehaviourTreeStatus DoOnAttack(TimeData t)
    {
        // ���� �ִϸ��̼� ���
        anim.Play(AnimHash.ATTACK_1);

        skill = GameManager.Instance.combatSystem.GetSkill();
        skill.Init(transform.position, this.gameObject.layer, testSkillData);

        skill.Enable();

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
    /// ��, �� �̵� �Է� ó��
    /// </summary>
    private BehaviourTreeStatus DoDefaultInputProc(TimeData t)
    {
        if (onInteration)
        {
            return BehaviourTreeStatus.Failure;
        }

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

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (interactionCurrent == null
            || onInteration == true)
        {
            return;
        }

        onInteration = true;
        interactionCurrent.StartInteraction(async () =>
        {
            interactionCurrent = null;
            await Awaitable.WaitForSecondsAsync(0.1f); // ��ȣ�ۿ� ������ ����
            onInteration = false;
        });
    }

    void ICombatable.TakeHit(float damage)
    {
        throw new System.NotImplementedException();
    }
}
