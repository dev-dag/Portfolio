using System;
using System.Collections.Generic;
using FluentBehaviourTree;
using NUnit.Framework;
using UnityEngine;

// Dungeon 보스 처치 후 시네마틱에 사용될 오브젝트
public class Cat6 : NPC
{
    private struct AnimHash
    {
        public static int ITCH = Animator.StringToHash("Itch");
        public static int WALK = Animator.StringToHash("Walk");
    }

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float moveSpeed = 4f;

    private IBehaviourTreeNode behaviourTreeRoot;
    private bool doBehaviourTree = false;

    protected override void Start()
    {
        base.Start();

        animator.Play(AnimHash.ITCH);
        behaviourTreeRoot = GetBehaviourTree();

        WaitOneSec(() => doBehaviourTree = true);
    }

    protected override void Update()
    {
        base.Update();

        if (doBehaviourTree)
        {
            behaviourTreeRoot.Tick(new TimeData(Time.deltaTime));
        }
    }

    private IBehaviourTreeNode GetBehaviourTree()
    {
        BehaviourTreeBuilder builder = new BehaviourTreeBuilder();
        builder.Selector(string.Empty)
            .Do(string.Empty, (t) =>
            {
                Vector2 dist = Player.Current.transform.position - this.transform.position;

                if (dist.x <= 3f)
                {
                    animator.Play(AnimHash.ITCH);
                    doBehaviourTree = false; // BT 종료
                    WaitOneSec(() =>
                    {
                        StartDialog(() =>
                        {
                            WaitOneSec(() =>
                            {
                                GameManager.Instance.globalUI.Fade.FadeOut();
                            });
                        });
                    });

                    return BehaviourTreeStatus.Success;
                }
                else
                {
                    animator.Play(AnimHash.WALK);

                    if (dist.x > 0.01f)
                    {
                        rigidBody.linearVelocityX = moveSpeed;
                    }
                    else if (dist.x < -0.01f)
                    {
                        rigidBody.linearVelocityX = -moveSpeed;
                    }

                    return BehaviourTreeStatus.Running;
                }
            })
        .End();

        return builder.Build();
    }

    private async Awaitable WaitOneSec(Action callback, float time = 1f)
    {
        await Awaitable.WaitForSecondsAsync(time);

        callback?.Invoke();
    }
}
