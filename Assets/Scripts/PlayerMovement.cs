using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] [Min(0f)] private float moveSpeed, jumpHeight, jumpBuffer;

    private float velX;
    private float timeSinceJump;
    private bool jumpPressed = false;
    private bool jumpReleased = false;

    private void Update()
    {
        if (timeSinceJump < jumpBuffer && IsGrounded() && jumpPressed)
        {
            Jump();
            jumpPressed = false;
        }
        if (jumpReleased && rb.velocity.y > 0f)
        {
            StopJump();
            jumpReleased = false;
        }

        if (IsGrounded()) jumpReleased = false;
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
        timeSinceJump = 0f;
        if (context.started) jumpPressed = true;
        if (context.canceled) jumpReleased = true;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapBox(new Vector3(transform.position.x, transform.position.y - transform.localScale.y / 2, transform.position.z), new Vector2(transform.localScale.x, 0.5f), 0f, groundLayer);
    }
}
