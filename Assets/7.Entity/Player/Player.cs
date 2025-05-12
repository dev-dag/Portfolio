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
public class Player : Entity, ICombatable
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
    public bool BlockInput { get; set; } = false; // 시네마틱 연출을 위해서 플레이어의 입력을 차단하기 위한 플래그  
    public bool OnInteration { get => onInteration; }

    [Space(30f)]
    [SerializeField] private SpriteRenderer render;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform footTr;

    [Space(30f)]
    [SerializeField] private float interactableDistance = 2f;

    [Space(30f)]
    [SerializeField] private AudioClip runSFX;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip landingSFX;
    [SerializeField] private AudioClip deadSFX;

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

    private AudioPlayer audioPlayer;

    private void Awake()
    {
        if (Current == null)
        {
            Current = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }
        
    private void Update()
    {
        if (IsInit == false)
        {
            return;
        }

        BT_Root.Tick(new TimeData(Time.deltaTime));

        CheckInteraction();

        GroundCheck();
    }

    private void OnEnable()
    {
        if (IsInit == false)
        {
            return;
        }

        interactAction.performed += OnInteract;
        quickSlot_0_Action.performed += OnQuickSlot_0;
        quickSlot_1_Action.performed += OnQuickSlot_1;
        quickSlot_2_Action.performed += OnQuickSlot_2;
    }

    private void OnDisable()
    {
        if (IsInit == false)
        {
            return;
        }

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
        return hp > 0 ? false : true;
    }

    /// <summary>
    /// 플레이어의 현재 체력을 올려주는 함수.
    /// </summary>
    public void IncreaseHP(int amount)
    {
        hp += amount;

        // HP UI 반영
        GameManager.Instance.gameUI.PlayerInfoPreview.Increase(amount);
    }

    public void EquipWeapon(Weapon weapon)
    {
        EquipedWeapon = weapon;

        if (weapon == null)
        {
            weaponCache = null;

            GameManager.Instance.gameUI.PlayerInfoPreview.SetWeaponSprite(null); // Info Preview UI 변경
            GameManager.Instance.gameUI.SkillView.SetSkill(null); // 스킬 View UI 설정
        }
        else
        {
            weaponCache = weapon;

            GameManager.Instance.gameUI.PlayerInfoPreview.SetWeaponSprite(weaponCache.Item.IconSprite); // Info Preview UI 변경
            GameManager.Instance.gameUI.SkillView.SetSkill(weaponCache); // 스킬 View UI 설정
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

        Collider2D closeObject = hitArr[0].collider;
        Vector2 closeDistance = hitArr[0].collider.transform.position - this.transform.position;
         
        foreach (RaycastHit2D hit in hitArr) // 상호작용 가능한 오브젝트 중, 더 가까운 오브젝트 선별
        {
            IInteractable interactableHit = hit.collider.GetComponent<IInteractable>();

            if (interactableHit.IsInteractable() == false) // 상호작용 불가능한 대상 제외
            {
                continue;
            }

            Vector2 distanceToPlayer = hit.collider.transform.position - this.transform.position; // 플레이어와의 거리 계산

            if (closeDistance.sqrMagnitude > distanceToPlayer.sqrMagnitude) 
            {
                closeObject = hit.collider;
                closeDistance = distanceToPlayer;
            }
        }

        IInteractable interactable = closeObject.GetComponent<IInteractable>();

        if (interactionCurrent == null)
        {
            interactionCurrent = interactable;

            interactionCurrent.SetInteractionGuide(true);
        }
        else if (interactable != interactionCurrent) // 현재 상호작용 가능한 대상 갱신 후 반환
        {
            interactionCurrent.SetInteractionGuide(false);
            interactionCurrent = interactable;

            interactionCurrent.SetInteractionGuide(true);
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

    public override void Init()
    {
        base.Init();

        CurrentAnimationState = AnimationState.Idle;
        BlockInput = false;
        IsOnGround = true;
        interactionCurrent = null;
        onInteration = false;
        noTakeDamage = false;
        weaponCache = null;

        if (blinkAwaiter != null)
        {
            blinkAwaiter.Cancel();
            blinkAwaiter = null;
        }

        render.color = new Color(render.color.r, render.color.g, render.color.b, 1f);

        InputActionMap actionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("Player");

        moveAction = actionMap.FindAction("Move");
        jumpAction = actionMap.FindAction("Jump");
        
        quickSlot_0_Action = actionMap.FindAction("UseQuickSlot_0");
        quickSlot_1_Action = actionMap.FindAction("UseQuickSlot_1");
        quickSlot_2_Action = actionMap.FindAction("UseQuickSlot_2");

        quickSlot_0_Action.performed -= OnQuickSlot_0;
        quickSlot_1_Action.performed -= OnQuickSlot_1;
        quickSlot_2_Action.performed -= OnQuickSlot_2;
        quickSlot_0_Action.performed += OnQuickSlot_0;
        quickSlot_1_Action.performed += OnQuickSlot_1;
        quickSlot_2_Action.performed += OnQuickSlot_2;

        // UI 입력 초기화
        InputActionMap UI_ActionMap = GameManager.Instance.globalInputActionAsset.FindActionMap("UI");
        interactAction = UI_ActionMap.FindAction("Interact");
        interactAction.performed -= OnInteract;
        interactAction.performed += OnInteract;

        GameManager.Instance.gameUI.PlayerInfoPreview.Init(hp, null); // 체력 UI 설정

        // 데이터 기반으로 인벤토리 채우기
        Inventory inventory = GameManager.Instance.gameUI.Inventory;
        InstanceData data = GameManager.Instance.InstanceData;
        
        if (data.EquippedWeaponID != -1 && inventory.WeaponSlot.IsEmpty == false)
        {
            EquipWeapon(GameManager.Instance.InstanceData.Items[data.EquippedWeaponID] as Weapon);
        }

        AttachCamera(); // 카메라 팔로우 설정

        if (audioPlayer == null)
        {
            audioPlayer = GameManager.Instance.audioSystem.GetUnManagedAudioPlayer(AudioSystem.AudioType.SFX); // 오디오 플레이어 로드
        }

        if (BT_Root == null)
        {
            BT_Root = MakeBehaviourTree();
        }
    }

    private IBehaviourTreeNode MakeBehaviourTree()
    {
        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();

        builder = builder
            .Selector("플레이어 BT")
                .Do(string.Empty, (t) => // 사망 체크 및 처리
                {
                    if (isDead == true)
                    {
                        return BehaviourTreeStatus.Success;
                    }
                    else if (hp <= 0)
                    {
                        GameManager.Instance.audioSystem.PlaySFX(AudioSystem.AudioType.SFX, deadSFX); // SFX 재생

                        animator.Play(AnimHash.DEAD); // 사망 애니메이션 재생
                        CurrentAnimationState = AnimationState.Dead;

                        isDead = true; // 생존 여부 필드 설정

                        return BehaviourTreeStatus.Success;
                    }
                    else
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                })

                .Sequence(string.Empty) // 무기가 장착된 상태에서 공격 처리
                    .Condition(string.Empty, (t) => weaponCache != null)
                    .Selector(string.Empty)
                        .Do(string.Empty, (t) =>
                        {
                            if (weaponCache == null)
                            {
                                return BehaviourTreeStatus.Failure;
                            }

                            var result = weaponCache.GetSkill_0_BehaviourTree(this).Tick(t);

                            if (result != BehaviourTreeStatus.Failure)
                            {
                                audioPlayer.Stop();
                            }

                            return result;
                        })

                        .Do(string.Empty, (t) =>
                        {
                            if (weaponCache == null)
                            {
                                return BehaviourTreeStatus.Failure;
                            }

                            var result = weaponCache.GetSkill_1_BehaviourTree(this).Tick(t);

                            if (result != BehaviourTreeStatus.Failure)
                            {
                                audioPlayer.Stop();
                            }

                            return result;
                        })

                        .Do(string.Empty, (t) =>
                        {
                            if (weaponCache == null)
                            {
                                return BehaviourTreeStatus.Failure;
                            }

                            var result = weaponCache.GetSkill_2_BehaviourTree(this).Tick(t);

                            if (result != BehaviourTreeStatus.Failure)
                            {
                                audioPlayer.Stop();
                            }

                            return result;
                        })
                    .End()
                .End()

                .Sequence(string.Empty)
                    .Do(string.Empty, (t) =>
                    {
                        if (moveAction.IsPressed() && moveAction.IsInProgress() && BlockInput == false) // 좌우 이동 처리
                        {
                            Vector2 dir = moveAction.ReadValue<Vector2>();
                            float newX = dir.x;

                            rigidBody.linearVelocityX = newX * Info.Speed;

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

                        if (jumpAction.IsPressed() && jumpAction.IsInProgress() && rigidBody.linearVelocityY.IsAlmostEqaul(0f) && BlockInput == false) // 점프 키가 눌린 경우
                        {
                            audioPlayer.SetLoop(false);
                            audioPlayer.Play(jumpSFX); // SFX 재생

                            rigidBody.linearVelocityY += Info.JumpPower;
                        }

                        return BehaviourTreeStatus.Success;
                    })
                    .Do(string.Empty, (t) =>
                    {
                        if (RigidBody.linearVelocityY.IsAlmostEqaul(0f) && CurrentAnimationState == AnimationState.Fall) // 착지 SFX를 위한 분기
                        {
                            audioPlayer.SetLoop(false);
                            audioPlayer.Play(landingSFX); // SFX 재생
                        }

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
                            if (CurrentAnimationState != AnimationState.Run) // 처음에 재생되는 Run 애니메이션인 경우
                            {
                                audioPlayer.SetLoop();
                                audioPlayer.Play(runSFX); // SFX 재생
                            }

                            animator.Play(AnimHash.RUN);
                            CurrentAnimationState = AnimationState.Run;
                        }
                        else
                        {
                            audioPlayer.Stop();

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

        GameManager.Instance.gameUI.Inventory.Hide();

        animator.Play(AnimHash.IDLE);
        CurrentAnimationState = AnimationState.Idle;
        audioPlayer.Stop();

        onInteration = true;
        BlockInput = true;
        interactionCurrent.StartInteraction(async () =>
        {
            interactionCurrent = null;
            await Awaitable.WaitForSecondsAsync(0.1f); // 상호작용 딜레이 설정
            BlockInput = false;
            onInteration = false;
        });
    }

    private void OnQuickSlot_0(InputAction.CallbackContext context)
    {
        ExclusiveItemSlot quickSlot = GameManager.Instance.gameUI.QuickSlot.GetQuickSlotByIndex(0);

        if (quickSlot.IsEmpty == false)
        {
            ItemContainer itemContainer = GameManager.Instance.InstanceData.Items[quickSlot.ItemID];

            (itemContainer as Potion).Drink();
        }
    }

    private void OnQuickSlot_1(InputAction.CallbackContext context)
    {
        ExclusiveItemSlot quickSlot = GameManager.Instance.gameUI.QuickSlot.GetQuickSlotByIndex(1);

        if (quickSlot.IsEmpty == false)
        {
            ItemContainer itemContainer = GameManager.Instance.InstanceData.Items[quickSlot.ItemID];

            (itemContainer as Potion).Drink();
        }
    }

    private void OnQuickSlot_2(InputAction.CallbackContext context)
    {
        ExclusiveItemSlot quickSlot = GameManager.Instance.gameUI.QuickSlot.GetQuickSlotByIndex(2);

        if (quickSlot.IsEmpty == false)
        {
            ItemContainer itemContainer = GameManager.Instance.InstanceData.Items[quickSlot.ItemID];

            (itemContainer as Potion).Drink();
        }
    }

    void ICombatable.TakeHit(int damage, Entity hitter)
    {
        if (noTakeDamage)
        {
            return;
        }

        TakeHitVFX vfx = GameManager.Instance.combatSystem.GetTakeHitVFX();
        vfx.Init(this.transform.position + Vector3.up * 2f);
        vfx.Enable();

        hp -= damage;

        if (blinkAwaiter != null)
        {
            blinkAwaiter.Cancel();
            render.color = new Color(render.color.r, render.color.g, render.color.b, 1f);
            noTakeDamage = false;
        }

        blinkAwaiter = BlinkSpriteRender();

        // HP UI 반영
        GameManager.Instance.gameUI.PlayerInfoPreview.Decrease((int)damage);
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
