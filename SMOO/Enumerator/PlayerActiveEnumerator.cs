using SMOO.Client;

namespace SMOO.Enumerator;

internal ref struct PlayerActiveEnumerator : IPlayerEnumerator<PlayerActiveEnumerator>
{
    private readonly ReadOnlySpan<Player> _players;
    private int _index;
    private Player _current = null!;
    public readonly Player Current => _current;
    public readonly PlayerActiveEnumerator GetEnumerator() => this;

    public PlayerActiveEnumerator(ReadOnlySpan<Player> players)
    {
        _players = players;
        _index = -1;
    }

    public bool MoveNext()
    {
        while (++_index < _players.Length)
        {
            Player? player = _players[_index];

            if (player != null)
            {
                _current = player;
                return true;
            }
        }

        return false;
    }

    public readonly void Dispose()
    {

    }
}
