using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ObjectContextWatcher))]

public class NpcController : NetworkBehaviour, IGameCharacter
{
    public GameCharacterType Type => GameCharacterType.Npc;

    public int Index { get; set; } = -1;

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
        Index = _gameManager.characters.Count - 1;
    }

    public override void OnNetworkDespawn()
    {
        if (_gameManager.characters.Contains(this))
            _gameManager.characters.Remove(this);

        Index = -1;
    }

    public string GetContext() => _scm.GetContext(gameObject);
    
    public bool Equals(IGameCharacter other) => other is not null && Type == other.Type && Index == other.Index;
}
