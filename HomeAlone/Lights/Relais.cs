namespace HomeAlone.Lights;

internal readonly record struct Relais
{
    public byte ModuleId { get; init; }
    public byte ChannelId { get; init; }

    public Relais(byte moduleId, byte channelId)
    {
        if (channelId < 1)
            throw new ArgumentOutOfRangeException(nameof(channelId), "ChannelId must be at least 1.");
        ModuleId = moduleId;
        ChannelId = channelId;
    }

    
    /// <summary>
    /// Parses a character span representing a relais identifier in the format "ModuleId.ChannelId" and returns a
    /// corresponding <see cref="Relais"/> instance.
    /// </summary>
    /// <param name="value">A read-only span of characters containing the relais identifier to parse. The expected format is
    /// "ModuleId.ChannelId", where both parts are numeric values.</param>
    /// <returns>A <see cref="Relais"/> instance constructed from the parsed module and channel identifiers.</returns>
    /// <exception cref="FormatException">Thrown if <paramref name="value"/> does not conform to the expected format or contains invalid numeric values.</exception>
    public static Relais Parse(ReadOnlySpan<char> value)
    {
        // E.g. "3.2"
        int index = 0;
        byte moduleId = 0;
        byte channelId = 0;

        value = value.Trim();

        foreach (var range in value.Split('.'))
        {
            if(index == 0)
            {
                if (!byte.TryParse(value[range], out moduleId))
                    throw new FormatException("Invalid ModuleId format.");
            }
            else if(index == 1)
            {
                if (!byte.TryParse(value[range], out channelId))
                    throw new FormatException("Invalid ChannelId format.");
            }
            else
            {
                throw new FormatException("Invalid relais format. Expected format: 'ModuleId.ChannelId'.");
            }
            index++;
        }

        if(index != 2)
            throw new FormatException("Invalid relais format. Expected format: 'ModuleId.ChannelId'.");


        return new Relais(moduleId, channelId);
    }
}
