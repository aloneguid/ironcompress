using System;
using System.Buffers;
using IronCompress;
using NetBox.Generator;
using Xunit;

namespace IronCompress.Test;

public class IronTest
{
   [Theory]
   [InlineData(Codec.Snappy, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })]
   [InlineData(Codec.Snappy, new byte[]
        {
         1, 2, 3, 4, 5, 6, 7, 8,
         1, 2, 3, 4, 5, 6, 7, 8,
         1, 2, 3, 4, 5, 6, 7, 8,
         1, 2, 3, 4, 5, 6, 7, 8,
         1, 2, 3, 4, 5, 6, 7, 8,
         1, 2, 3, 4, 5, 6, 7, 8,
         1, 2, 3, 4, 5, 6, 7, 8})]
   [InlineData(Codec.Snappy, null)]
   public void EncodeDecodeTest(Codec codec, byte[] input)
   {
      if (input == null) input = RandomGenerator.GetRandomBytes(1000, 10000);

      byte[]? encoded = null;
      byte[]? decoded = null;

      try
      {
         encoded = Iron.Compress(codec,
            input,
            ArrayPool<byte>.Shared,
            out int encodeSize);

         decoded = Iron.Decompress(codec,
            encoded.AsSpan(0, encodeSize),
            ArrayPool<byte>.Shared,
            out int decodeSize);

         Assert.Equal(input.Length, decodeSize);

      }
      finally
      {
         if (encoded != null)
            ArrayPool<byte>.Shared.Return(encoded);

         if (decoded != null)
            ArrayPool<byte>.Shared.Return(decoded);
      }
   }
}