using UnityEngine;
using FluentBehaviourTree;
using UnityEngine.InputSystem;
using System.Timers;
using Unity.Cinemachine;
using UnityEngine.Rendering;

/// <summary>
/// ���ÿ� 2�� �̻��� �ν��Ͻ��� �����ϸ� �ȵǴ� Ŭ����.
/// �ݵ�� �ʿ��� ���, �ݵ�� �ϳ��� �ν��Ͻ��� Disable �� ���¿��� �Ѵ�.
/// </summary>
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
    [SerializeField] private SpriteRenderer render;
    [SerializeField] private PlayerInfo info;
    [SerializeField] private Rigidbody2D RB;
    [SerializeField] private Animator anim;

    [Space(20f)]
    [SerializeField] private float interactableDistance = 2f;

    private IBehaviourTreeNode BT_Root;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction skill_0_Action;
    private InputAction quickSlot_0_Action;
    private InputAction quickSlot_1_Action;
    private InputAction quickSlot_2_Action;
    private InputAction interactAction;

    private IInteractable interactionCurrent; // ���� ���ͷ��� ������ ���
    private bool onInteration = false;
    private bool noTakeDamage = false;
    private float blinkSpeed = 30f;
    private float blinkTime = 0.5f;
    Awaitable blinkAwaiter = null;

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
        quickSlot_0_Action.performed += OnQuickSlot_0;
        quickSlot_1_Action.performed += OnQuickSlot_1;
        quickSlot_2_Action.performed += OnQuickSlot_2;
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteract;
        quickSlot_0_Action.performed -= OnQuickSlot_0;
        quickSlot_1_Action.performed -= OnQuickSlot_1;
        quickSlot_2_Action.performed -= OnQuickSlot_2;
    }

    /// <summary>
    /// ���� ���� ��ȯ
    /// </summary>
    public bool IsAlive()
    {
        return info.HP > 0f ? false : true;
    }

    /// <summary>
    /// �÷��̾��� ���� ü���� �÷��ִ� �Լ�.
    /// </summary>
    public void IncreaseHP(float amount)
    {
        info.HP += amount;
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
        skill_0_Action = actionMap.FindAction("UseSkill_0");
        
        quickSlot_0_Action = actionMap.FindAction("UseQuickSlot_0");
        quickSlot_1_Action = actionMap.FindAction("UseQuickSlot_1");
        quickSlot_2_Action = actionMap.FindAction("UseQuickSlot_2");

        // UI �Է� �ʱ�ȭ
        InputActionMap UI_ActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("UI");
        interactAction = UI_ActionMap.FindAction("Interact");

        info.Init();

        BT_Root = MakeBehaviourTree();
    }

    private IBehaviourTreeNode MakeBehaviourTree()
    {
        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

        builder = builder
            .Selector("�÷��̾� BT")
                .Condition(string.Empty, (t) => info.isDead)

                .Sequence(string.Empty)
                    .Condition("���� ���", (t) => info.HP <= 0f)
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
                                    .Condition("���� ��ư�� ���� ���", (t) => skill_0_Action.IsPressed() && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") == false)
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
        info.isDead = true;

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

        RB.linearVelocityY += info.jumpPower;

        return BehaviourTreeStatus.Running;
    }

    Skill skill;

    private BehaviourTreeStatus DoOnAttack(TimeData t)
    {
        // ���� �ִϸ��̼� ���
        anim.Play(AnimHash.ATTACK_1);

        skill = GameManager.Instance.combatSystem.GetSkill();
        skill.Init(transform.position, transform.rotation, gameObject.layer, testSkillData, this);

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

            RB.linearVelocityX = newX * info.speed;

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

    private void OnQuickSlot_0(InputAction.CallbackContext context)
    {
        ExclusiveItemSlot quickSlot = GameManager.Instance.uiManager.quickSlot.GetQuickSlotByIndex(0);

        if (quickSlot.ItemID != null)
        {
            ItemContainer itemContainer = GameManager.Instance.uiManager.inventory.Items[quickSlot.ItemID.Value];

            (itemContainer as Potion).Drink();
        }
    }

    private void OnQuickSlot_1(InputAction.CallbackContext context)
    {
        ExclusiveItemSlot quickSlot = GameManager.Instance.uiManager.quickSlot.GetQuickSlotByIndex(1);

        if (quickSlot.ItemID != null)
        {
            ItemContainer itemContainer = GameManager.Instance.uiManager.inventory.Items[quickSlot.ItemID.Value];

            (itemContainer as Potion).Drink();
        }
    }

    private void OnQuickSlot_2(InputAction.CallbackContext context)
    {
        ExclusiveItemSlot quickSlot = GameManager.Instance.uiManager.quickSlot.GetQuickSlotByIndex(2);

        if (quickSlot.ItemID != null)
        {
            ItemContainer itemContainer = GameManager.Instance.uiManager.inventory.Items[quickSlot.ItemID.Value];

            (itemContainer as Potion).Drink();
        }
    }

    void ICombatable.TakeHit(float damage, BaseObject hitter)
    {
        if (noTakeDamage)
        {
            return;
        }

        TakeHitVFX vfx = GameManager.Instance.combatSystem.GetTakeHitVFX();
        vfx.Init(this.transform.position + Vector3.up * 2f);
        vfx.Enable();

        info.HP -= damage;

        if (blinkAwaiter != null)
        {
            blinkAwaiter.Cancel();
            render.color = new Color(render.color.r, render.color.g, render.color.b, 1f);
            noTakeDamage = false;
        }

        blinkAwaiter = BlinkSpriteRender();
    }

    private async Awaitable BlinkSpriteRender()
    {
        float time = Time.time + blinkTime;

        while (Time.time < time)
        {
            noTakeDamage = true;

            render.color = new Color(render.color.r, render.color.g, render.color.b, Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed)));

            await Awaitable.NextFrameAsync();
        }

        render.color = new Color(render.color.r, render.color.g, render.color.b, 1f);

        noTakeDamage = false;
    }
}
