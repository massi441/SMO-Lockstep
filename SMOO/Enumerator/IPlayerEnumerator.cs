using SMOO.Client;

namespace SMOO.Enumerator;

internal interface IPlayerEnumerator<TSelf> : ISpanEnumerator<Player, TSelf> where TSelf : allows ref struct;
