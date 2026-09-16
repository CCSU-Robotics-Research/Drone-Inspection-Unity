using System;
using System.Collections.Generic;

/// <summary>
/// Parses the ground station's frame stream from the
/// ground station repo, which was a little-endian length
/// with a JPEG payload.
/// 
/// Mirrors the FrameAssembler from the ground station,
/// responsible for JPEG frame assembly.
/// </summary>
public class FrameAssembler
{
    public const int MaxFrameBytes = 8 * 1024 * 1024;

    private readonly List<byte> buffer = new List<byte>();

    /// <summary>
    /// Consume <paramref name="count"/> bytes from
    /// <paramref name="chunk"/> and return every complete
    /// JPEG payload. Throws InvalidOperationException when length is
    /// too long, indicating the stream is corrupted.
    /// </summary>
    /// <param name="chunk">The array of bytes of payload.</param>
    /// <param name="count">The number of bytes inputted.</param>
    /// <returns>A list of complete JPEG payloads.</returns>
    public List<byte[]> ExtractPayloads(byte[] chunk, int count)
    {
        for (int i = 0; i < count; i++)
        {
            buffer.Add(chunk[i]);
        }

        var payloads = new List<byte[]>();

        while (buffer.Count >= 4)
        {
            uint length = (uint)(
                buffer[0]
                | (buffer[1] << 8)
                | (buffer[2] << 16)
                | (buffer[3] << 24));

            if (length == 0 || length > MaxFrameBytes)
            {
                throw new InvalidOperationException(
                    $"frame length {length} is too long. Possible corruption.");
            }

            int total = 4 + (int)length;
            if (buffer.Count < total)
            {
                break;
            }

            var payload = new byte[length];
            buffer.CopyTo(4, payload, 0, (int)length);
            payloads.Add(payload);
            buffer.RemoveRange(0, total);
        }

        return payloads;
    }
}
