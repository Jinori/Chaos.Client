using Chaos.Packets.Abstractions;

namespace Chaos.Client.Networking.Titles;

public sealed record TitleListArgs : IPacketSerializable
{
    public byte ActiveIndex { get; set; } = 0;
    public List<string> Titles { get; set; } = [];
}
