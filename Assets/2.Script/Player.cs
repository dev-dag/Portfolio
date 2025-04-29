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
/// ���ÿ� 2�� �̻��� �ν��Ͻ��� �����ϸ� �ȵǴ� Ŭ����.
/// �ݵ�� �ʿ��� ���, �ݵ�� �ϳ��� �ν��Ͻ��� Disable �� ���¿��� �Ѵ�.
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

    [Space(20f)]
    [SerializeField] private SpriteRenderer render;
    [SerializeField] private PlayerInfo info;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Animator animator;

    [Space(20f)]
    [SerializeField] private float interactableDistance = 2f;

    private IBehaviourTreeNode BT_Root;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction quickSlot_0_Action;
    private InputAction quickSlot_1_Action;
    private InputAction quickSlot_2_Action;
    private InputAction interactAction;

    private IInteractable interactionCurrent; // ���� ���ͷ��� ������ ���
    private bool onInteration = false;
    private bool noTakeDamage = false;
    private float blinkSpeed = 30f;
    private float blinkTime = 0.5f;
    private Awaitable blinkAwaiter = null;
    private Weapon weaponCache = null;

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
        return info.HP > 0 ? false : true;
    }

    /// <summary>
    /// �÷��̾��� ���� ü���� �÷��ִ� �Լ�.
    /// </summary>
    public void IncreaseHP(int amount)
    {
        info.HP += amount;

        // HP UI �ݿ�
        GameManager.Instance.uiManager.playerInfoPreview.Increase(amount);
    }

    public async Task EquipWeapon(Weapon weapon)
    {
        EquipedWeapon = weapon;

        if (weapon == null)
        {
            weaponCache = null;

            GameManager.Instance.uiManager.playerInfoPreview.SetWeaponSprite(null); // Info Preview UI ����
            GameManager.Instance.uiManager.skillView.SetSkill(null); // ��ų View UI ����
        }
        else
        {
            weaponCache = weapon;

            GameManager.Instance.uiManager.playerInfoPreview.SetWeaponSprite(weaponCache.Item.IconSprite); // Info Preview UI ����
            GameManager.Instance.uiManager.skillView.SetSkill(weaponCache); // ��ų View UI ����
        }
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
        
        quickSlot_0_Action = actionMap.FindAction("UseQuickSlot_0");
        quickSlot_1_Action = actionMap.FindAction("UseQuickSlot_1");
        quickSlot_2_Action = actionMap.FindAction("UseQuickSlot_2");

        // UI �Է� �ʱ�ȭ
        InputActionMap UI_ActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("UI");
        interactAction = UI_ActionMap.FindAction("Interact");

        info.Init();
        GameManager.Instance.uiManager.playerInfoPreview.Init(info.HP, null); // ü�� UI ����

        BT_Root = MakeBehaviourTree();
    }

    private IBehaviourTreeNode MakeBehaviourTree()
    {
        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

        builder = builder
            .Selector("�÷��̾� BT")
                .Do(string.Empty, (t) => // ��� üũ �� ó��
                {
                    if (info.isDead == true)
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (info.HP <= 0)
                    {
                        animator.Play(AnimHash.DEAD); // ��� �ִϸ��̼� ���
                        CurrentAnimationState = AnimationState.Dead;

                        info.isDead = true; // ���� ���� �ʵ� ����

                        return BehaviourTreeStatus.Success;
                    }
                    else
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                })

                .Sequence(string.Empty) // ���Ⱑ ������ ���¿��� ���� ó��
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
                    .Do(string.Empty, (t) =>
                    {
                        if (moveAction.IsPressed() && moveAction.IsInProgress()) // �¿� �̵� ó��
                        {
                            Vector2 dir = moveAction.ReadValue<Vector2>();
                            float newX = dir.x;

                            rigidBody.linearVelocityX = newX * info.speed;

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

                                rigidBody.transform.rotation = Quaternion.Euler(rigidBody.transform.rotation.x, newRotY, rigidBody.transform.rotation.z);
                            }
                        }

                        if (jumpAction.IsPressed() && jumpAction.IsInProgress() && rigidBody.linearVelocityY.IsAlmostEqaul(0f)) // ���� Ű�� ���� ���
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

    /// <summary>
    /// ��, �� �̵� �� ���� ó��
    /// </summary>
    private BehaviourTreeStatus DoDefaultInputProc(TimeData t)
    {
        if (onInteration) // ��ȣ�ۿ� �߿� �۵����� ����.
        {
            return BehaviourTreeStatus.Failure;
        }

        if (moveAction.IsPressed() && moveAction.IsInProgress()) // �¿� �̵� ó��
        {
            Vector2 dir = moveAction.ReadValue<Vector2>();
            float newX = dir.x;

            rigidBody.linearVelocityX = newX * info.speed;
            animator.Play(AnimHash.RUN); // Run �ִϸ��̼� ���
            CurrentAnimationState = AnimationState.Run;

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

                rigidBody.transform.rotation = Quaternion.Euler(rigidBody.transform.rotation.x, newRotY, rigidBody.transform.rotation.z);
            }
        }

        if (jumpAction.IsPressed() && jumpAction.IsInProgress() && rigidBody.linearVelocityY.IsAlmostEqaul(0f)) // ���� Ű�� ���� ���
        {
            animator.Play(AnimHash.JUMP); // ���� �ִϸ��̼� ���
            CurrentAnimationState = AnimationState.Jump;

            rigidBody.linearVelocityY += info.jumpPower;
        }
        else if (rigidBody.linearVelocityY < -0.1f) // �߶� ���� ���
        {
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != AnimHash.FALL)
            {
                animator.Play(AnimHash.FALL); // �߶� �ִϸ��̼� ���
                CurrentAnimationState = AnimationState.Fall;
            }
        }
        else if (RigidBody.linearVelocityX.IsAlmostEqaul(0f))
        {
            animator.Play(AnimHash.IDLE);
            CurrentAnimationState = AnimationState.Idle;
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

        // HP UI �ݿ�
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
