using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Receives commands from the State Machine and applies forces to the Rigidbody2D
/// </summary>
public class PlayerMotor : MonoBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private bool isFacingRight = true;

    public Vector2 Velocity => rb.velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    public void Jump(float jumpForce)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void Move(Vector2 moveInput, float currentSpeed, float accleration, float deaccleration)
    {
        // 1. Calculate target speed
        float targetSpeed = moveInput.x * currentSpeed;

        // 2. Calculate difference between targetSpeed and currentSpeed
        float speedDif = targetSpeed - rb.velocity.x;

        // 3. Choose acceleration rate
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f && Mathf.Sign(targetSpeed) == Mathf.Sign(speedDif)) ? accleration : deaccleration;

        // 4. Apply force
        float movement = Mathf.Min(Mathf.Abs(speedDif) * accelRate, accelRate * currentSpeed) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
        if (moveInput.x > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }

    public bool IsGrounded(float distance, float width, LayerMask groundedLayer)
    {
        Vector2 boxCenter = new Vector2(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y);

        Vector2 boxSize = new Vector2(width, 0.05f);

        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, distance, groundedLayer);
        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        // TryGet
        if (capsuleCollider == null)
        {
            capsuleCollider = GetComponent<CapsuleCollider2D>();
        }

        PlayerSettings settings = GetComponent<PlayerSettings>();
        if (settings == null)
        {
            return; 
        }

        float distance = settings.Detection.GroundedCheckDistance;
        float width = settings.Detection.GroundedCheckWidth;
        LayerMask groundedLayer = settings.Detection.GroundedLayer;

        Vector2 boxCenter = new Vector2(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y);
        Vector2 boxSize = new Vector2(width, 0.05f);

        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, distance, groundedLayer);

        Gizmos.color = (hit.collider != null) ? Color.green : Color.red;

        Gizmos.DrawWireCube(boxCenter, boxSize);

        Vector2 endBoxCenter = boxCenter + Vector2.down * distance;
        Gizmos.DrawWireCube(endBoxCenter, boxSize);

        Gizmos.DrawLine(new Vector2(boxCenter.x - boxSize.x / 2, boxCenter.y), new Vector2(endBoxCenter.x - boxSize.x / 2, endBoxCenter.y));
        Gizmos.DrawLine(new Vector2(boxCenter.x + boxSize.x / 2, boxCenter.y), new Vector2(endBoxCenter.x + boxSize.x / 2, endBoxCenter.y));
    }
}
