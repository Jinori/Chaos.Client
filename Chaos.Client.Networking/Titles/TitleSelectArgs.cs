using Chaos.Packets.Abstractions;

namespace Chaos.Client.Networking.Titles;

public sealed record TitleSelectArgs : IPacketSerializable
{
    public byte Index { get; set; } = 0;
}
