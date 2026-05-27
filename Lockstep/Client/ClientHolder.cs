namespace Lockstep.Client;

internal class ClientHolder : IClientHolder
{
    private List<PlayerClient> _players;

    public ClientHolder(int size = 4)
    {
        _players = new List<PlayerClient>(size);
    }

    public void AddPlayer(PlayerClient player)
    {
        if (ContainsPlayer(player))
        {
            return;
        }

        _players.Add(player);
    }

    public void RemovePlayer(PlayerClient player)
    {
        _players.Remove(player);
    }

    private bool ContainsPlayer(PlayerClient player)
    {
        foreach (PlayerClient p in _players)
        {
            if (p.Endpoint == player.Endpoint)
            {
                return true;
            }
        }

        return false;
    }
}
