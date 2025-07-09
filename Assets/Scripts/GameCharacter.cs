public interface IGameCharacter
{
    public GameCharacterType Type { get; }
}

public enum GameCharacterType
{
    Seeker,
    Hider,
    Npc
}
