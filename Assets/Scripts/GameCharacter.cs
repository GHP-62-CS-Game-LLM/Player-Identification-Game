using System;
using Unity.Netcode;

public interface IGameCharacter : IEquatable<IGameCharacter>
{
    public GameCharacterType Type { get; }
    
    public int Index { get; set; }
}

public enum GameCharacterType
{
    Seeker,
    Hider,
    Npc
}
