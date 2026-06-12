using System.Net;

namespace SMOO.Client;

internal readonly struct PlayerId
{
    public required IPEndPoint Endpoint { get; init; }
    public required Guid SessionId { get; init; }

    public static bool operator ==(PlayerId left, PlayerId right)
    {
        return left.Endpoint.Equals(right.Endpoint) && left.SessionId == right.SessionId;
    }

    public static bool operator !=(PlayerId left, PlayerId right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is PlayerId id)
        {
            return this == id;
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
