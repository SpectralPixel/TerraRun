using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] [Min(0f)] private float moveSpeed, jumpHeight, jumpBuffer, stepDistance;

    private Rigidbody2D rb;

    private float velX;
    private float timeSinceJump;
    private bool jumpPressed = false;
    private bool jumpReleased = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (timeSinceJump < jumpBuffer && IsGrounded() && jumpPressed)
        {
            Jump();
            jumpPressed = false;
            jumpReleased = false;
        }
        if (jumpReleased && rb.velocity.y > 0f)
        {
            StopJump();
            jumpReleased = false;
        }

        if (IsGrounded()) jumpReleased = false;

        if (CanStep()) transform.position = new Vector2(transform.position.x, transform.position.y + 0.2f);
    }

    private void FixedUpdate()
    {
        timeSinceJump += Time.fixedDeltaTime;

        rb.velocity = new Vector2(velX * moveSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
    }

    private void StopJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 2); // Go up slower for dynamic jump
    }

    public void Move(InputAction.CallbackContext context)
    {
        velX = context.ReadValue<Vector2>().x;
    }

    public void JumpButton(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpPressed = true;
            timeSinceJump = 0f;
        }
        if (context.canceled) jumpReleased = true;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapBox(new Vector3(transform.position.x, transform.position.y - transform.localScale.y / 2, transform.position.z), new Vector2(transform.localScale.x, 0.5f), 0f, groundLayer);
    }

    private bool CanStep()
    {
        return 
            // if you are moving and
            Mathf.Abs(velX) > 0.1f &&
            // there is a tile next to you that you should be stepping up
            Physics2D.OverlapBox(new Vector3(transform.position.x, (transform.position.y - transform.localScale.y / 2) + 0.5f, transform.position.z), new Vector2(transform.localScale.x + stepDistance, 0.9f), 0f, groundLayer) &&
            // and there isn't another tile above the tile you want to be stepping up (you're not trying to step up a two tile tall wall)
            !Physics2D.OverlapBox(new Vector3(transform.position.x, (transform.position.y - transform.localScale.y / 2) + 1.5f, transform.position.z), new Vector2(transform.localScale.x + stepDistance, 0.9f), 0f, groundLayer);
    }
}