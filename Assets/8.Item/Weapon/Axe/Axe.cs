using Database_Table;
using FluentBehaviourTree;
using UnityEngine;

public class Axe : Weapon
{
    private Skill skill_0;
    private Skill skill_1;
    private Skill skill_2;

    private IBehaviourTreeNode skill_0_behaviourTree_cache;
    private IBehaviourTreeNode skill_1_behaviourTree_cache;
    private IBehaviourTreeNode skill_2_behaviourTree_cache;

    public override bool Init(Item item, int amount)
    {
        if (base.Init(item, amount) == false)
        {
            return false;
        }

        skill_0 = new Skill(WeaponInfo.SkillData_0, WeaponInfo.damage);
        skill_1 = new Skill(WeaponInfo.SkillData_1, WeaponInfo.damage);
        skill_2 = new Skill(WeaponInfo.SkillData_2, WeaponInfo.damage);

        return true;
    }

    public override Skill[] GetSkills()
    {
        return new Skill[3] { skill_0, skill_1, skill_2 };
    }

    public override IBehaviourTreeNode GetSkill_0_BehaviourTree(Player player)
    {
        if (skill_0_behaviourTree_cache != null)
        {
            return skill_0_behaviourTree_cache;
        }

        var skillAction = GameManager.Instance.globalInputActionAsset.FindActionMap("Player").FindAction("UseSkill_0");
        bool skillActivated = false;

        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();
        builder.Selector(string.Empty)
            .Do(string.Empty, (t) => // 재생 중일 때 처리
            {
                if (player.CurrentAnimationState == Player.AnimationState.Attack_0)
                {
                    if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // 애니메이션이 끝난 경우
                    {
                        player.Animator.Play(Player.AnimHash.IDLE);
                        player.CurrentAnimationState = Player.AnimationState.Idle;

                        // On end of animation
                        {

                        }

                        skillActivated = false; // 람다로 사용되기 때문에 외부 변수 초기화

                        return BehaviourTreeStatus.Success;
                    }
                    else if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.6f) // 공격 FX 시점
                    {
                        if (skillActivated == false)
                        {
                            GameManager.Instance.audioSystem.PlaySFX(AudioSystem.AudioType.SFX, skill_0.Data.SFX_Clips[0]); // SFX 재생

                            skill_0.TryOperateWithFollow(player.transform, player.transform.rotation, LayerMask.NameToLayer(GameManager.PLAYER_EXCLUSIVE_LAYER_NAME), player); // 스킬 시전 시도
                            skillActivated = true;
                        }

                        return BehaviourTreeStatus.Running;
                    }
                    else
                    {
                        return BehaviourTreeStatus.Running;
                    }
                }
                else
                {
                    return BehaviourTreeStatus.Failure;
                }
            })

            .Do(string.Empty, (t) =>
            {
                if (skill_0.IsOperatable() == false) // 0번 스킬 쿨타임 체크
                {
                    return BehaviourTreeStatus.Failure;
                }
                else if (skillAction.IsPressed() && player.BlockInput == false) // 입력이 있는 경우
                {
                    if (player.CurrentAnimationState != Player.AnimationState.Attack_1
                        && player.CurrentAnimationState != Player.AnimationState.Attack_2)
                    {
                        player.RigidBody.linearVelocityX = 0f;
                        player.Animator.Play(WeaponInfo.AnimationStateName_0[0]); // 애니메이션 재생

                        player.CurrentAnimationState = Player.AnimationState.Attack_0; // 상태머신 갱신

                        // Progress
                        {

                        }

                        return BehaviourTreeStatus.Success;
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
        .End();

        skill_0_behaviourTree_cache = builder.Build();
        return skill_0_behaviourTree_cache;
    }

    private const float UP_VELOCITY_X = 40f;
    private const float UP_VELOCITY_Y = 70f;
    private const float DROP_VELOCITY_Y = -70f;
    public override IBehaviourTreeNode GetSkill_1_BehaviourTree(Player player)
    {
        if (skill_1_behaviourTree_cache != null)
        {
            return skill_1_behaviourTree_cache;
        }

        var skillAction = GameManager.Instance.globalInputActionAsset.FindActionMap("Player").FindAction("UseSkill_1");
        bool skillActivated = false; // 바닥에 착지했을 때 스킬을 오퍼레이트 했는지 체크하기 위한 플래그
        bool dropSpeedApplied = false; // 상승 후 내려찍기 시 가속이 적용됐는지 체크하기 위한 플래그
        int finishStateNameHash = Animator.StringToHash(WeaponInfo.AnimationStateName_1[1]);

        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();
        builder.Selector(string.Empty)
            .Do(string.Empty, (t) => // 재생 중일 때 처리
            {
                if (player.CurrentAnimationState == Player.AnimationState.Attack_1)
                {
                    if (dropSpeedApplied == false
                        && player.RigidBody.linearVelocityY < 0f && player.IsOnGround == false) // 추락 시작 시 가속 처리
                    {
                        player.RigidBody.linearVelocityY = DROP_VELOCITY_Y;
                        dropSpeedApplied = true;
                    }

                    if (player.RigidBody.linearVelocityY <= 0f // 지면과 가까워지고 플랫폼에 발이 붙어있는 판정일 때
                        && player.IsOnGround)
                    {
                        if (skillActivated == false)
                        {
                            GameManager.Instance.audioSystem.PlaySFX(AudioSystem.AudioType.SFX, skill_1.Data.SFX_Clips[1]); // SFX 재생

                            player.Animator.Play(WeaponInfo.AnimationStateName_1[1]);
                            skill_1.TryOperate(player.transform.position, player.transform.rotation, LayerMask.NameToLayer(GameManager.PLAYER_EXCLUSIVE_LAYER_NAME), player); // 스킬 시전 시도
                            skillActivated = true;
                        }

                        if (player.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == finishStateNameHash // 피니쉬 애니메이션이 끝난 경우
                            && player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                        {
                            player.Animator.Play(Player.AnimHash.IDLE);
                            player.CurrentAnimationState = Player.AnimationState.Idle;

                            skillActivated = false; // 람다로 사용되기 때문에 외부 변수 초기화
                            dropSpeedApplied = false; // 람다로 사용되기 때문에 외부 변수 초기화

                            return BehaviourTreeStatus.Success;
                        }
                        else
                        {
                            return BehaviourTreeStatus.Running;
                        }
                    }
                    else
                    {
                        return BehaviourTreeStatus.Running;
                    }
                }
                else
                {
                    return BehaviourTreeStatus.Failure;
                }
            })

            .Do(string.Empty, (t) =>
            {
                if (skill_1.IsOperatable() == false) // 1번 스킬 쿨타임 체크
                {
                    return BehaviourTreeStatus.Failure;
                }
                else if (skillAction.IsPressed() && player.BlockInput == false) // 입력이 있는 경우
                {
                    if (player.IsOnGround == false) // 공중에 있는 경우 시전 불가
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else if (player.CurrentAnimationState != Player.AnimationState.Attack_0
                        && player.CurrentAnimationState != Player.AnimationState.Attack_2)
                    {
                        player.RigidBody.linearVelocityX = 0f;
                        player.Animator.Play(WeaponInfo.AnimationStateName_1[0]); // 애니메이션 재생
                        player.CurrentAnimationState = Player.AnimationState.Attack_1; // 상태머신 갱신

                        GameManager.Instance.audioSystem.PlaySFX(AudioSystem.AudioType.SFX, skill_1.Data.SFX_Clips[0]); // SFX 재생

                        // Progress
                        {
                            if (player.transform.rotation.eulerAngles.y == 0f)
                            {
                                player.RigidBody.linearVelocityX = UP_VELOCITY_X;
                            }
                            else if (player.transform.rotation.eulerAngles.y == 180f)
                            {
                                player.RigidBody.linearVelocityX = -UP_VELOCITY_X;
                            }
                            
                            player.RigidBody.linearVelocityY = UP_VELOCITY_Y;
                        }

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
        .End();

        skill_1_behaviourTree_cache = builder.Build();
        return skill_1_behaviourTree_cache;
    }

    public override IBehaviourTreeNode GetSkill_2_BehaviourTree(Player player)
    {
        if (skill_2_behaviourTree_cache != null)
        {
            return skill_2_behaviourTree_cache;
        }
        var skillAction = GameManager.Instance.globalInputActionAsset.FindActionMap("Player").FindAction("UseSkill_2");
        bool skillActivated = false;

        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();
        builder.Selector(string.Empty)
            .Do(string.Empty, (t) => // 재생 중일 때 처리
            {
                if (player.CurrentAnimationState == Player.AnimationState.Attack_2)
                {
                    if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // 애니메이션이 끝난 경우
                    {
                        player.Animator.Play(Player.AnimHash.IDLE);
                        player.CurrentAnimationState = Player.AnimationState.Idle;

                        // On end of animation
                        {

                        }

                        skillActivated = false; // 람다로 사용되기 때문에 외부 변수 초기화

                        return BehaviourTreeStatus.Success;
                    }
                    else if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.33f) // 공격 FX 시점
                    {
                        if (skillActivated == false)
                        {
                            GameManager.Instance.audioSystem.PlaySFX(AudioSystem.AudioType.SFX, skill_2.Data.SFX_Clips[0]); // SFX 재생

                            skill_2.TryOperate(player.transform.position, player.transform.rotation, LayerMask.NameToLayer(GameManager.PLAYER_EXCLUSIVE_LAYER_NAME), player); // 스킬 시전 시도
                            skillActivated = true;
                        }

                        return BehaviourTreeStatus.Running;
                    }
                    else
                    {
                        return BehaviourTreeStatus.Running;
                    }
                }
                else
                {
                    return BehaviourTreeStatus.Failure;
                }
            })

            .Do(string.Empty, (t) =>
            {
                if (skill_2.IsOperatable() == false) // 2번 스킬 쿨타임 체크
                {
                    return BehaviourTreeStatus.Failure;
                }
                else if (skillAction.IsPressed() && player.BlockInput == false) // 입력이 있는 경우
                {
                    if (player.IsOnGround == false) // 공중에 있는 경우 시전 불가
                    {
                        return BehaviourTreeStatus.Failure;
                    }
                    else if (player.CurrentAnimationState != Player.AnimationState.Attack_0
                        && player.CurrentAnimationState != Player.AnimationState.Attack_1)
                    {
                        player.RigidBody.linearVelocityX = 0f;
                        player.Animator.Play(WeaponInfo.AnimationStateName_2[0]); // 애니메이션 재생
                        player.CurrentAnimationState = Player.AnimationState.Attack_2; // 상태머신 갱신

                        // Progress
                        {

                        }

                        return BehaviourTreeStatus.Success;
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
        .End();

        skill_2_behaviourTree_cache = builder.Build();
        return skill_2_behaviourTree_cache;
    }
}
