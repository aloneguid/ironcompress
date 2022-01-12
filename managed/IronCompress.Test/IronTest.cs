using System;
using System.Buffers;
using IronCompress;
using NetBox.Generator;
using Xunit;

namespace IronCompress.Test;

public class IronTest
{
   private static readonly Iron _iron = new Iron(ArrayPool<byte>.Shared);

   [Theory]
   [InlineData(Codec.Snappy)]
   [InlineData(Codec.Zstd)]
   [InlineData(Codec.Gzip)]
   [InlineData(Codec.Brotli)]
   [InlineData(Codec.LZO)]
   [InlineData(Codec.LZ4)]
   public void EncodeDecodeTest(Codec codec)
   {
      int minLength = RandomGenerator.GetRandomInt(10, 1000);
      int maxLength = RandomGenerator.GetRandomInt(minLength, minLength + 1000);
      byte[] input = RandomGenerator.GetRandomBytes(minLength, maxLength);

      using (Result compressed = _iron.Compress(codec, input.AsSpan()))
      {
         using (Result uncompressed = _iron.Decompress(codec, compressed, input.Length))
         {
            Assert.Equal(input, uncompressed.AsSpan().ToArray());
         }
      }
   }
}