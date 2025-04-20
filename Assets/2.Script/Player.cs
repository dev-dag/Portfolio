using UnityEngine;
using FluentBehaviourTree;
using UnityEngine.InputSystem;
using System.Timers;
using Unity.Cinemachine;
using UnityEngine.Rendering;

/// <summary>
/// 동시에 2개 이상의 인스턴스가 존재하면 안되는 클래스.
/// 반드시 필요한 경우, 반드시 하나의 인스턴스는 Disable 된 상태여야 한다.
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

    private IInteractable interactionCurrent; // 현재 인터렉션 가능한 대상
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
    /// 생존 여부 반환
    /// </summary>
    public bool IsAlive()
    {
        return info.HP > 0f ? false : true;
    }

    /// <summary>
    /// 플레이어의 현재 체력을 올려주는 함수.
    /// </summary>
    public void IncreaseHP(float amount)
    {
        info.HP += amount;
    }

    /// <summary>
    /// 상호작용 UI 사용 가능 여부 체크
    /// </summary>
    private void CheckInteraction()
    {
        if (onInteration) // 상호작용 중인경우 반환
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

            if (interactableHit.IsInteractable() == false) // 상호작용 불가능한 대상 제외
            {
                continue;
            }

            if (interactionCurrent == null)
            {
                interactionCurrent = interactableHit;

                interactionCurrent.SetInteractionGuide(true);
            }
            else if (interactableHit != interactionCurrent) // 현재 상호작용 가능한 대상 갱신 후 반환
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

        // UI 입력 초기화
        InputActionMap UI_ActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("UI");
        interactAction = UI_ActionMap.FindAction("Interact");

        info.Init();

        BT_Root = MakeBehaviourTree();
    }

    private IBehaviourTreeNode MakeBehaviourTree()
    {
        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

        builder = builder
            .Selector("플레이어 BT")
                .Condition(string.Empty, (t) => info.isDead)

                .Sequence(string.Empty)
                    .Condition("죽은 경우", (t) => info.HP <= 0f)
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
                                    .Condition("공격 버튼을 누른 경우", (t) => skill_0_Action.IsPressed() && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") == false)
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
                .End()

                .Do("Run/Idle 애니메이션", DoOnIdleAndRun)
            .End();

            return builder.Build();
    }

    private BehaviourTreeStatus DoOnDead(TimeData t)
    {
        // 사망 애니메이션 재생
        anim.Play(AnimHash.DEAD);

        // 생존 여부 필드 설정
        info.isDead = true;

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

        RB.linearVelocityY += info.jumpPower;

        return BehaviourTreeStatus.Running;
    }

    Skill skill;

    private BehaviourTreeStatus DoOnAttack(TimeData t)
    {
        // 공격 애니메이션 재생
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
    /// 좌, 우 이동 입력 처리
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
            await Awaitable.WaitForSecondsAsync(0.1f); // 상호작용 딜레이 설정
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
