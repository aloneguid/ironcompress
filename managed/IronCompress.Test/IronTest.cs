using System;
using System.Buffers;
using IronCompress;
using Xunit;

namespace IronCompress.Test;

public class IronTest {
    private static readonly Iron _iron = new Iron(ArrayPool<byte>.Shared);
    private readonly Random _rnd = new Random(DateTime.UtcNow.Millisecond);

    [Theory]
    [InlineData(Codec.Snappy)]
    [InlineData(Codec.Zstd)]
    [InlineData(Codec.Gzip)]
    [InlineData(Codec.Brotli)]
    [InlineData(Codec.LZO)]
    [InlineData(Codec.LZ4)]
    public void EncodeDecodeTest(Codec codec) {
        byte[] input = new byte[_rnd.Next(100, 10000)];
        _rnd.NextBytes(input);

        using(Result compressed = _iron.Compress(codec, input.AsSpan())) {
            using(Result uncompressed = _iron.Decompress(codec, compressed, input.Length)) {
                Assert.Equal(input, uncompressed.AsSpan().ToArray());
            }
        }
    }
}