using System.Net;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IPacketSender
{
    Result<Error> Send(EndPoint destination, RentedBuffer buffer);
    Result<Error> SendReliably(Player receiver, RentedBuffer buffer, IReliablePacketStore reliableStore, byte maxRetries = Config.MaxRetries);
}
