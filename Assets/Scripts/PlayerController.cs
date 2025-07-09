using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5.0f;
    
    private InputAction _moveAction;
    
    void Start()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) Move();
    }

    private void Move()
    {
        transform.position = new Vector3(Random.Range(-8.0f, 8.0f), Random.Range(-4.0f, 4.0f));
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 moveValue = _moveAction.ReadValue<Vector2>() * (moveSpeed * Time.deltaTime);

        transform.position += new Vector3(moveValue.x, moveValue.y, 0.0f);
    }
}
