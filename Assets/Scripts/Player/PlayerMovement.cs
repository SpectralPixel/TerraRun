using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] [Min(0f)] private float moveSpeed, jumpHeight, jumpBuffer, stepDistance;

    [SerializeField] private bool drawGizmos;

    private Rigidbody2D rb;

    private float velX;
    private float timeSinceJump;
    private bool jumpPressed = false;
    private bool jumpReleased = false;
    private bool placingBlocksInPlayer = false;

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

        while (CanStep()) transform.position = new Vector2(transform.position.x, transform.position.y + 0.2f);
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
        return Physics2D.OverlapBox(new Vector3(transform.position.x, transform.position.y - transform.localScale.y / 2, transform.position.z), new Vector2(transform.localScale.x * 0.95f, 0.5f), 0f, groundLayer);
    }

    private bool CanStep()
    {
        return 
            // if you are moving and
            Mathf.Abs(velX) > 0.1f &&
            // there is a tile next to you that you should be stepping up (size only set to 0.9f to avoid trying to step up when walking normally)
            Physics2D.OverlapBox(new Vector3(transform.position.x, (transform.position.y - transform.localScale.y / 2f) + 0.5f, transform.position.z), new Vector2(transform.localScale.x + stepDistance, 0.9f), 0f, groundLayer) &&
            // and there aren't any tiles solid tiles up to head height (so the player doesn't try to step up a two block wall or into a gap they won't fit into)
            !Physics2D.OverlapBox(new Vector3(transform.position.x, ((transform.position.y + transform.localScale.y / 2f + 0.7f) + ((transform.position.y - transform.localScale.y / 2f) + 1.1f)) / 2f, transform.position.z), new Vector2(transform.localScale.x + stepDistance, (transform.position.y + transform.localScale.y / 2 + 0.7f) - ((transform.position.y - transform.localScale.y / 2) + 1.1f)), 0f, groundLayer) &&
            !placingBlocksInPlayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Tile Selection" && GetComponent<PlayerHand>().GetMouseDown())
        {
            placingBlocksInPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Tile Selection")
        {
            placingBlocksInPlayer = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            // Jumping
            Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y - transform.localScale.y / 2, transform.position.z), new Vector2(transform.localScale.x - 0.05f, 0.5f));
            // Stepping
            Gizmos.DrawWireCube(new Vector3(transform.position.x, (transform.position.y - transform.localScale.y / 2f) + 0.5f, transform.position.z), new Vector2(transform.localScale.x + stepDistance, 0.9f));
            Gizmos.DrawWireCube(new Vector3(transform.position.x, ((transform.position.y + transform.localScale.y / 2f + 0.7f) + ((transform.position.y - transform.localScale.y / 2f) + 1.1f)) / 2f, transform.position.z), new Vector2(transform.localScale.x + stepDistance, (transform.position.y + transform.localScale.y / 2 + 0.7f) - ((transform.position.y - transform.localScale.y / 2) + 1.1f)));
        }
    }
}