using Database_Table;
using FluentBehaviourTree;
using UnityEngine;

public class Sword : Weapon
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

                        return BehaviourTreeStatus.Success;
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
                else if (skillAction.IsPressed()) // 입력이 있는 경우
                {
                    if (player.CurrentAnimationState != Player.AnimationState.Attack_1
                        && player.CurrentAnimationState != Player.AnimationState.Attack_2)
                    {
                        player.Animator.Play(WeaponInfo.AnimationStateName_0); // 애니메이션 재생
                        player.CurrentAnimationState = Player.AnimationState.Attack_0; // 상태머신 갱신

                        skill_0.TryOperate(player.transform.position, player.transform.rotation, player.gameObject.layer, player); // 스킬 시전 시도
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

    private const float JUMP_POWER = 60f;
    public override IBehaviourTreeNode GetSkill_1_BehaviourTree(Player player)
    {
        if (skill_1_behaviourTree_cache != null)
        {
            return skill_1_behaviourTree_cache;
        }

        var skillAction = GameManager.Instance.globalInputActionAsset.FindActionMap("Player").FindAction("UseSkill_1");

        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();
        builder.Selector(string.Empty)
            .Do(string.Empty, (t) => // 재생 중일 때 처리
            {
                if (player.CurrentAnimationState == Player.AnimationState.Attack_1)
                {
                    if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // 애니메이션이 끝난 경우
                    {
                        player.Animator.Play(Player.AnimHash.IDLE);
                        player.CurrentAnimationState = Player.AnimationState.Idle;

                        return BehaviourTreeStatus.Success;
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
                else if (skillAction.IsPressed()) // 입력이 있는 경우
                {
                    if (player.CurrentAnimationState != Player.AnimationState.Attack_0
                        && player.CurrentAnimationState != Player.AnimationState.Attack_2)
                    {
                        player.Animator.Play(WeaponInfo.AnimationStateName_1); // 애니메이션 재생
                        player.CurrentAnimationState = Player.AnimationState.Attack_1; // 상태머신 갱신
                        player.RigidBody.linearVelocityY = JUMP_POWER;

                        skill_1.TryOperate(player.transform.position, player.transform.rotation, player.gameObject.layer, player); // 스킬 시전 시도
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

        skill_1_behaviourTree_cache = builder.Build();
        return skill_1_behaviourTree_cache;
    }

    private const float DASH_POWER = 50f;
    public override IBehaviourTreeNode GetSkill_2_BehaviourTree(Player player)
    {
        if (skill_2_behaviourTree_cache != null)
        {
            return skill_2_behaviourTree_cache;
        }
        var skillAction = GameManager.Instance.globalInputActionAsset.FindActionMap("Player").FindAction("UseSkill_2");

        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();
        builder.Selector(string.Empty)
            .Do(string.Empty, (t) => // 재생 중일 때 처리
            {
                if (player.CurrentAnimationState == Player.AnimationState.Attack_2)
                {
                    if (player.MoveAction.IsPressed()
                        && player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f) // 애니메이션 종료 직전 이동 입력이 있는 경우 후 모션 스킵
                    {
                        player.Animator.Play(Player.AnimHash.IDLE);
                        player.CurrentAnimationState = Player.AnimationState.Idle;

                        return BehaviourTreeStatus.Failure;
                    }
                    else if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) // 애니메이션이 끝난 경우
                    {
                        player.Animator.Play(Player.AnimHash.IDLE);
                        player.CurrentAnimationState = Player.AnimationState.Idle;

                        return BehaviourTreeStatus.Success;
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
                else if (skillAction.IsPressed()) // 입력이 있는 경우
                {
                    if (player.CurrentAnimationState != Player.AnimationState.Attack_0
                        && player.CurrentAnimationState != Player.AnimationState.Attack_1)
                    {
                        player.Animator.Play(WeaponInfo.AnimationStateName_2); // 애니메이션 재생
                        player.CurrentAnimationState = Player.AnimationState.Attack_2; // 상태머신 갱신
                        player.RigidBody.linearVelocityY = 0f;

                        if (player.transform.eulerAngles.y == 0f)
                        {
                            player.RigidBody.linearVelocityX = DASH_POWER;
                        }
                        else
                        {
                            player.RigidBody.linearVelocityX = -DASH_POWER;
                        }

                        skill_2.TryOperate(player.transform.position, player.transform.rotation, player.gameObject.layer, player); // 스킬 시전 시도
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
