using SMOO.Client;

namespace SMOO.Enumerator;

internal ref struct PlayerIgnoreEnumerator : IPlayerEnumerator<PlayerIgnoreEnumerator>
{
    private PlayerActiveEnumerator _activeEnumerator;
    private readonly Player _ignoredPlayer;

    public readonly Player Current => _activeEnumerator.Current;
    public PlayerIgnoreEnumerator GetEnumerator() => this;

    public PlayerIgnoreEnumerator(ReadOnlySpan<Player> players, Player ignordPlayer)
    {
        _activeEnumerator = new PlayerActiveEnumerator(players);
        _ignoredPlayer = ignordPlayer;
    }

    public bool MoveNext()
    {
        bool result = _activeEnumerator.MoveNext();
        if (_activeEnumerator.Current == _ignoredPlayer)
        {
            return _activeEnumerator.MoveNext();
        }

        return result;
    }

    public readonly void Dispose()
    {

    }
}
