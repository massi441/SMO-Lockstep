using System.Net;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IPacketSender
{
    Result<Error> Send(EndPoint destination, RentedBuffer buffer);
    void SendReliably(Player receiver, RentedBuffer buffer, Room room, RefCounter refCounter, byte maxRetries = Config.MaxRetries);
    void SendReliably(Player receiver, RentedBuffer buffer, Room room, byte maxRetries = Config.MaxRetries);
}
