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

        using(DataBuffer compressed = _iron.Compress(codec, input.AsSpan())) {
            using(DataBuffer uncompressed = _iron.Decompress(codec, compressed, input.Length)) {
                Assert.Equal(input, uncompressed.AsSpan().ToArray());
            }
        }
    }

    [Theory]
    [InlineData(Codec.Snappy)]
    [InlineData(Codec.Zstd)]
    [InlineData(Codec.Gzip)]
    [InlineData(Codec.Brotli)]
    [InlineData(Codec.LZO)]
    [InlineData(Codec.LZ4)]
    public void EncodeDecodeSmallTest(Codec codec) {
        byte[] input = new byte[4];
        _rnd.NextBytes(input);

        using(DataBuffer compressed = _iron.Compress(codec, input.AsSpan())) {
            using(DataBuffer uncompressed = _iron.Decompress(codec, compressed, input.Length)) {
                Assert.Equal(input, uncompressed.AsSpan().ToArray());
            }
        }
    }

    /*[Theory]
    [InlineData(Codec.Snappy)]
    [InlineData(Codec.Zstd)]
    [InlineData(Codec.Gzip)]
    [InlineData(Codec.Brotli)]
    [InlineData(Codec.LZO)]
    [InlineData(Codec.LZ4)]
    public void EncodeDecodeUnknownSizeTest(Codec codec) {
        byte[] input = new byte[_rnd.Next(100, 10000)];
        _rnd.NextBytes(input);

        using(DataBuffer compressed = _iron.Compress(codec, input.AsSpan())) {
            using(DataBuffer uncompressed = _iron.Decompress(codec, compressed, null)) {
                Assert.Equal(input, uncompressed.AsSpan().ToArray());
            }
        }
    }*/
}