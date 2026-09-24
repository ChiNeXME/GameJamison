using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastMooncake.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        [Header("Top-Down Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        [SerializeField, Min(0f)] private float acceleration = 35f;
        [SerializeField, Min(0f)] private float deceleration = 45f;

        private Rigidbody2D body;
        private InputAction moveAction;
        private Vector2 moveInput;
        private Vector2 facingDirection = Vector2.down;

        public Vector2 MoveInput => moveInput;
        public Vector2 FacingDirection => facingDirection;
        public bool IsMoving => moveInput.sqrMagnitude > 0.01f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;

            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector(mode=2)")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector(mode=2)")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");
            moveAction.AddBinding("<Gamepad>/dpad");
        }

        private void Reset()
        {
            Rigidbody2D attachedBody = GetComponent<Rigidbody2D>();
            attachedBody.gravityScale = 0f;
            attachedBody.freezeRotation = true;
            attachedBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void OnEnable()
        {
            moveAction.Enable();
        }

        private void OnDisable()
        {
            moveAction.Disable();

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        private void OnDestroy()
        {
            moveAction.Dispose();
        }

        private void Update()
        {
            moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                facingDirection = ChooseCardinalDirection(moveInput);
            }
        }

        private void FixedUpdate()
        {
            Vector2 targetVelocity = moveInput * moveSpeed;
            float rate = IsMoving ? acceleration : deceleration;
            body.linearVelocity = Vector2.MoveTowards(
                body.linearVelocity,
                targetVelocity,
                rate * Time.fixedDeltaTime);
        }

        private static Vector2 ChooseCardinalDirection(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return direction.x >= 0f ? Vector2.right : Vector2.left;
            }

            return direction.y >= 0f ? Vector2.up : Vector2.down;
        }
    }
}
