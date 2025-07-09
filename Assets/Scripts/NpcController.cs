using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ObjectContextWatcher))]

public class NpcController : NetworkBehaviour, IGameCharacter
{
    public GameCharacterType Type => GameCharacterType.Npc;

    private SceneContextManager scm;

    public void Awake()
    {
        scm = FindAnyObjectByType<SceneContextManager>();
    }

    public string GetContext() => scm.GetContext(gameObject);
}
