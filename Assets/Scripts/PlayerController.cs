using System;
using Chat;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerController : NetworkBehaviour, IGameCharacter
{
    private GameCharacterType _type;
    public GameCharacterType Type
    {
        get => _type;
        set
        {
            switch (value)
            {
            case GameCharacterType.Seeker:
                HiderObj.SetActive(false);
                SeekerObj.SetActive(true);

                break;
            case GameCharacterType.Hider:
                HiderObj.SetActive(true);
                SeekerObj.SetActive(false);

                break;
            case GameCharacterType.Npc:
                break;
            }
            
            _type = value;
        }
    }

    public GameObject SeekerObj;
    public GameObject HiderObj;

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
        if (IsOwner)
        {
            SetRandomPosition();
            _gameManager.localPlayer = this;
        }

        if (_gameManager.characters.Count >= 1)
            Type = GameCharacterType.Hider;
        else Type = GameCharacterType.Seeker;
        
        _gameManager.characters.Add(this);
        
        print($"Player Character Type: {Type}");
    }
    


    private void SetRandomPosition()
    {
        transform.position = new Vector3(Random.Range(-8.0f, 8.0f), Random.Range(-4.0f, 4.0f));
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (_gameManager.isInteracting) return;
            
        Vector2 moveValue = _moveAction.ReadValue<Vector2>() * (moveSpeed * Time.deltaTime);

        transform.position += new Vector3(moveValue.x, moveValue.y, 0.0f);

        if (_interactAction.WasPressedThisFrame())
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider.CompareTag("Interactable"))
            {
                _gameManager.StartInteractionRpc(
                    _gameManager.characters.IndexOf(this), // TODO: cache this?
                    _gameManager.characters.IndexOf(hit.transform.gameObject.GetComponentInParent<IGameCharacter>())
                );
            }
        }
    }
}
