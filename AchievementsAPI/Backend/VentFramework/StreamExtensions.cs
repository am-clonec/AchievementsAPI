using System.IO;

namespace AchievementsAPI.Backend;

internal static class StreamExtensions
{
    // https://github.com/NuclearPowered/Reactor/blob/master/Reactor/Utilities/Extensions/StreamExtensions.cs
    public static byte[] ReadFully(this Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
}
