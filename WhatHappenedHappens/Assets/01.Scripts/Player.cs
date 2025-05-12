using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;

    [Header("Movement")]
    private float moveX = 0f;
    private float moveSpeed = 5f;
    private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    private float groundCheckRadius = 0.2f;
    private bool isGrounded = false;
    private bool jumpPressed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");

        // 좌우 반전
        if (moveX != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveX);
            transform.localScale = scale;
        }

        // 점프 입력 감지
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            jumpPressed = true;
        }
    }

    void FixedUpdate()
    {
        // 바닥 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 좌우 이동
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // 점프 처리
        if (jumpPressed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpPressed = false;
        }
    }
}
