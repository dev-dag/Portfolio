using UnityEngine;
using FluentBehaviourTree;
using UnityEngine.InputSystem;
using System.Timers;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using Database_Table;
using UnityEngine.AddressableAssets;
using Unity.VisualScripting;
using System.Threading.Tasks;

/// <summary>
/// 동시에 2개 이상의 인스턴스가 존재하면 안되는 클래스.
/// 반드시 필요한 경우, 반드시 하나의 인스턴스는 Disable 된 상태여야 한다.
/// </summary>
public class Player : BaseObject, ICombatable
{
    public static Player Current { get; private set; }

    public struct AnimHash
    {
        public static readonly int IDLE = Animator.StringToHash("Idle");
        public static readonly int RUN = Animator.StringToHash("Run");
        public static readonly int JUMP = Animator.StringToHash("Jump");
        public static readonly int FALL = Animator.StringToHash("JumpFall");
        public static readonly int DEAD = Animator.StringToHash("Die");
    }

    public enum AnimationState
    {
        Idle = 0,
        Run,
        Jump,
        Fall,
        Dead,
        Attack_0,
        Attack_1,
        Attack_2,
    }

    public Weapon EquipedWeapon { get; private set; }
    public Rigidbody2D RigidBody { get => rigidBody; }
    public Animator Animator { get => animator; }
    public AnimationState CurrentAnimationState { get; set; }
    public InputAction MoveAction { get => moveAction; }
    public InputAction JumpAction { get => jumpAction; }
    public bool IsOnGround { get; private set; }
    public Transform FootTr { get => footTr; }
    public bool BlockInput { get; set; }
    public bool OnInteration { get => onInteration; }

    [Space(20f)]
    [SerializeField] private SpriteRenderer render;
    [SerializeField] private PlayerInfo info;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform footTr;

    [Space(20f)]
    [SerializeField] private float interactableDistance = 2f;

    private IBehaviourTreeNode BT_Root;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction quickSlot_0_Action;
    private InputAction quickSlot_1_Action;
    private InputAction quickSlot_2_Action;
    private InputAction interactAction;

    private IInteractable interactionCurrent; // 현재 인터렉션 가능한 대상
    private bool onInteration = false;
    private bool noTakeDamage = false;
    private float blinkSpeed = 30f;
    private float blinkTime = 0.5f;
    private Awaitable blinkAwaiter = null;
    private Weapon weaponCache = null;

    protected override void Awake()
    {
        base.Awake();

        if (Current == null)
        {
            Current = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }

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

        GroundCheck();
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
        return info.HP > 0 ? false : true;
    }

    /// <summary>
    /// 플레이어의 현재 체력을 올려주는 함수.
    /// </summary>
    public void IncreaseHP(int amount)
    {
        info.HP += amount;

        // HP UI 반영
        GameManager.Instance.uiManager.playerInfoPreview.Increase(amount);
    }

    public async Task EquipWeapon(Weapon weapon)
    {
        EquipedWeapon = weapon;

        if (weapon == null)
        {
            weaponCache = null;

            GameManager.Instance.uiManager.playerInfoPreview.SetWeaponSprite(null); // Info Preview UI 변경
            GameManager.Instance.uiManager.skillView.SetSkill(null); // 스킬 View UI 설정
        }
        else
        {
            weaponCache = weapon;

            GameManager.Instance.uiManager.playerInfoPreview.SetWeaponSprite(weaponCache.Item.IconSprite); // Info Preview UI 변경
            GameManager.Instance.uiManager.skillView.SetSkill(weaponCache); // 스킬 View UI 설정
        }
    }

    private void GroundCheck()
    {
        var hit = Physics2D.Raycast(footTr.position, Vector2.down, 1f, LayerMask.GetMask(GameManager.PLATFORM_LAYER_NAME));

        if (hit.collider != null)
        {
            IsOnGround = true;
        }
        else
        {
            IsOnGround = false;
        }

        return;
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

    public void AttachCamera()
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
        
        quickSlot_0_Action = actionMap.FindAction("UseQuickSlot_0");
        quickSlot_1_Action = actionMap.FindAction("UseQuickSlot_1");
        quickSlot_2_Action = actionMap.FindAction("UseQuickSlot_2");

        // UI 입력 초기화
        InputActionMap UI_ActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("UI");
        interactAction = UI_ActionMap.FindAction("Interact");

        info.Init();
        GameManager.Instance.uiManager.playerInfoPreview.Init(info.HP, null); // 체력 UI 설정

        BT_Root = MakeBehaviourTree();
    }

    private IBehaviourTreeNode MakeBehaviourTree()
    {
        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

        builder = builder
            .Selector("플레이어 BT")
                .Do(string.Empty, (t) => // 사망 체크 및 처리
                {
                    if (info.isDead == true)
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (info.HP <= 0)
                    {
                        animator.Play(AnimHash.DEAD); // 사망 애니메이션 재생
                        CurrentAnimationState = AnimationState.Dead;

                        info.isDead = true; // 생존 여부 필드 설정

                        return BehaviourTreeStatus.Success;
                    }
                    else
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                })

                .Condition(string.Empty, (t) => BlockInput)

                .Sequence(string.Empty) // 무기가 장착된 상태에서 공격 처리
                    .Condition(string.Empty, (t) => weaponCache != null)
                    .Selector(string.Empty)
                        .Do(string.Empty, (t) =>
                        {
                            if (weaponCache == null)
                            {
                                return BehaviourTreeStatus.Failure;
                            }

                            return weaponCache.GetSkill_0_BehaviourTree(this).Tick(t);
                        })

                        .Do(string.Empty, (t) =>
                        {
                            if (weaponCache == null)
                            {
                                return BehaviourTreeStatus.Failure;
                            }

                            return weaponCache.GetSkill_1_BehaviourTree(this).Tick(t);
                        })

                        .Do(string.Empty, (t) =>
                        {
                            if (weaponCache == null)
                            {
                                return BehaviourTreeStatus.Failure;
                            }

                            return weaponCache.GetSkill_2_BehaviourTree(this).Tick(t);
                        })
                    .End()
                .End()

                .Sequence(string.Empty)
                    .Condition(string.Empty, (t) => onInteration == false)
                    .Condition(string.Empty, (t) => BlockInput == false)
                    .Do(string.Empty, (t) =>
                    {
                        if (moveAction.IsPressed() && moveAction.IsInProgress()) // 좌우 이동 처리
                        {
                            Vector2 dir = moveAction.ReadValue<Vector2>();
                            float newX = dir.x;

                            rigidBody.linearVelocityX = newX * info.speed;

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

                                rigidBody.transform.rotation = Quaternion.Euler(rigidBody.transform.rotation.x, newRotY, rigidBody.transform.rotation.z);
                            }
                        }

                        if (jumpAction.IsPressed() && jumpAction.IsInProgress() && rigidBody.linearVelocityY.IsAlmostEqaul(0f)) // 점프 키가 눌린 경우
                        {
                            rigidBody.linearVelocityY += info.jumpPower;
                        }

                        return BehaviourTreeStatus.Success;
                    })
                    .Do(string.Empty, (t) =>
                    {
                        if (RigidBody.linearVelocityY > 0.01f)
                        {
                            animator.Play(AnimHash.JUMP);
                            CurrentAnimationState = AnimationState.Jump;
                        }
                        else if (RigidBody.linearVelocityY < -0.01f)
                        {
                            animator.Play(AnimHash.FALL);
                            CurrentAnimationState = AnimationState.Fall;
                        }
                        else if (RigidBody.linearVelocityX.IsAlmostEqaul(0f) == false)
                        {
                            animator.Play(AnimHash.RUN);
                            CurrentAnimationState = AnimationState.Run;
                        }
                        else
                        {
                            animator.Play(AnimHash.IDLE);
                            CurrentAnimationState = AnimationState.Idle;
                        }

                        return BehaviourTreeStatus.Success;
                    })
                .End()
            .End();

            return builder.Build();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (interactionCurrent == null
            || onInteration == true)
        {
            return;
        }

        GameManager.Instance.uiManager.inventory.Disable();

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

    void ICombatable.TakeHit(int damage, BaseObject hitter)
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

        // HP UI 반영
        GameManager.Instance.uiManager.playerInfoPreview.Decrease((int)damage);
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
