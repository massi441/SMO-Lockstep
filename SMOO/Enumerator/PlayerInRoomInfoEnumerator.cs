using SMOO.Client;

namespace SMOO.Enumerator;

internal ref struct PlayerInRoomInfoEnumerator : ISpanEnumerator<PlayerInRoomInfo, PlayerInRoomInfoEnumerator>
{
    private PlayerActiveEnumerator _playerEnumerator;
    private readonly Player? _exclude;
    private PlayerInRoomInfo _current;
    public readonly PlayerInRoomInfo Current => _current;
    public readonly PlayerInRoomInfoEnumerator GetEnumerator() => this;

    public PlayerInRoomInfoEnumerator(ReadOnlySpan<Player> players, Player? exclude = null)
    {
        _playerEnumerator = new PlayerActiveEnumerator(players);
        _exclude = exclude;
    }

    public bool MoveNext()
    {
        while (_playerEnumerator.MoveNext())
        {
            if (_playerEnumerator.Current == _exclude) continue;
            _current = new PlayerInRoomInfo(_playerEnumerator.Current);
            return true;
        }
        return false;
    }

    public readonly void Dispose() { }
}
