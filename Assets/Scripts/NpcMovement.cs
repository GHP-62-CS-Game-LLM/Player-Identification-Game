using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NpcMovement : MonoBehaviour
{
    internal Transform thisTransform;

    public float moveSpeed = 6f;
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 last = Vector3.right;

    public Vector2 decisionTime = new Vector2(1, 4);
    internal float decisionTimeCount = 0;

    internal Vector3[] moveDirections = new Vector3[] {
        Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.zero, Vector3.zero
    };
    internal int currentMoveDirection;

    private Vector3 direction;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        thisTransform = transform;

        decisionTimeCount = Random.Range(decisionTime.x, decisionTime.y);
        ChooseMoveDirection();
    }

    void Update()
    {
        direction = moveDirections[currentMoveDirection];

        if (animator)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
            animator.SetBool("moving", direction != Vector3.zero);
        }

        if (decisionTimeCount > 0)
        {
            decisionTimeCount -= Time.deltaTime;
        }
        else
        {
            decisionTimeCount = Random.Range(decisionTime.x, decisionTime.y);
            ChooseMoveDirection();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction.normalized * moveSpeed;
    }

    void ChooseMoveDirection()
    {
        currentMoveDirection = Random.Range(0, moveDirections.Length);
    }
}
