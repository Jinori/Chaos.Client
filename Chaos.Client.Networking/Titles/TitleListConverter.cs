using Chaos.IO.Memory;
using Chaos.Packets.Abstractions;

namespace Chaos.Client.Networking.Titles;

public sealed class TitleListConverter : PacketConverterBase<TitleListArgs>
{
    public override byte OpCode => TitleOpCodes.TitleList;

    public override TitleListArgs Deserialize(ref SpanReader reader)
    {
        var activeIndex = reader.ReadByte();
        var count = reader.ReadByte();
        var titles = new List<string>(count);

        for (var i = 0; i < count; i++)
            titles.Add(reader.ReadString8());

        return new TitleListArgs
        {
            ActiveIndex = activeIndex,
            Titles = titles
        };
    }

    public override void Serialize(ref SpanWriter writer, TitleListArgs args)
    {
        writer.WriteByte(args.ActiveIndex);
        writer.WriteByte((byte)args.Titles.Count);

        foreach (var title in args.Titles)
            writer.WriteString8(title);
    }
}
