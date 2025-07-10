using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ObjectContextWatcher))]

public class NpcController : NetworkBehaviour, IGameCharacter
{
    public GameCharacterType Type => GameCharacterType.Npc;

    private SceneContextManager _scm;
    private GameManager _gameManager;

    public void Awake()
    {
        _scm = FindAnyObjectByType<SceneContextManager>();
        _gameManager = FindAnyObjectByType<GameManager>();
    }

    public override void OnNetworkSpawn()
    {
        _gameManager.characters.Add(this);
    }

    public override void OnNetworkDespawn()
    {
        if (_gameManager.characters.Contains(this))
            _gameManager.characters.Remove(this);
    }

    public string GetContext() => _scm.GetContext(gameObject);
}
