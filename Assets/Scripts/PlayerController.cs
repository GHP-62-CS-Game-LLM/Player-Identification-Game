using Chat;
using Interactions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerController : NetworkBehaviour, IGameCharacter
{
    public GameCharacterType Type { get; set; } = GameCharacterType.Seeker;

    private Camera _camera;
    
    [Header("Movement")]
    public float moveSpeed = 5.0f;
    
    private InputAction _moveAction;
    private InputAction _interactAction;
    
    private GameManager _gameManager;
    //private ChatManager _chatManager;
    
    void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _interactAction = InputSystem.actions.FindAction("Interact");
        
        _gameManager = FindAnyObjectByType<GameManager>();
        //_chatManager = _gameManager.chatManager;

        _camera = FindAnyObjectByType<Camera>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) SetRandomPosition();

        if (_gameManager.players.Count >= 1)
            Type = GameCharacterType.Hider;
        
        _gameManager.players.Add(this);
        
        print($"Player Character Type: {Type}");
    }
    


    private void SetRandomPosition()
    {
        transform.position = new Vector3(Random.Range(-8.0f, 8.0f), Random.Range(-4.0f, 4.0f));
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (_gameManager.isInteracting.Value) return;
            
        Vector2 moveValue = _moveAction.ReadValue<Vector2>() * (moveSpeed * Time.deltaTime);

        transform.position += new Vector3(moveValue.x, moveValue.y, 0.0f);

        if (_interactAction.WasPressedThisFrame())
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider.CompareTag("Interactable"))
            {
                _gameManager.StartInteractionRpc(hit.transform.gameObject.GetComponent<IGameCharacter>().Type);
            }
        }
    }
}
