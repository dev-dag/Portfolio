using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : BaseObject
{
    [SerializeField] private Rigidbody2D targetRB;
    [SerializeField] private PlayerInfo playerInfo;
    [SerializeField] private InputActionAsset inputAction;

    private InputActionMap actionMap;
    private InputAction moveAction;

    protected override void Awake()
    {
        base.Awake();

        inputAction.Enable();

        actionMap = inputAction.FindActionMap("Player");

        moveAction = actionMap.FindAction("Move");

        var jumpAction = actionMap.FindAction("Jump");
        jumpAction.started += OnJump;
    }

    private void OnDestroy()
    {
        var jumpAction = actionMap.FindAction("Jump");
        jumpAction.started -= OnJump;
    }

    protected override void Update()
    {
        base.Update();

        // 이동 입력 처리
        if (moveAction.IsPressed())
        {
            Vector2 dir = moveAction.ReadValue<Vector2>();
            float newX = dir.x;

            targetRB.linearVelocityX = newX * playerInfo.speed;
            //targetRB.MovePosition(targetRB.position + (Vector2.right * newX * playerInfo.speed * Time.deltaTime));
        }
    }

    private void OnJump(InputAction.CallbackContext args)
    {
        targetRB.AddForceY(playerInfo.jumpPower, ForceMode2D.Impulse);
    }
}
