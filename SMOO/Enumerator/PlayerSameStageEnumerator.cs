using SMOO.Client;

namespace SMOO.Enumerator;

internal ref struct PlayerSameStageEnumerator : IPlayerEnumerator<PlayerSameStageEnumerator>
{
    private PlayerActiveEnumerator _activeEnumerator;
    private readonly Player _targetPlayer;
    public Player Current => _activeEnumerator.Current;
    public PlayerSameStageEnumerator GetEnumerator() => this;

    public PlayerSameStageEnumerator(ReadOnlySpan<Player> players, Player targetStagePlayer)
    {
        _activeEnumerator = new PlayerActiveEnumerator(players);
        _targetPlayer = targetStagePlayer;
    }

    public bool MoveNext()
    {
        while (_activeEnumerator.MoveNext())
        {
            if (_activeEnumerator.Current == _targetPlayer)
            {
                continue;
            }

            if (_activeEnumerator.Current.WorldInfo.CurrentStage == _targetPlayer.WorldInfo.CurrentStage)
            {
                return true;
            }
        }

        return false;
    }

    public readonly void Dispose()
    {

    }
}
