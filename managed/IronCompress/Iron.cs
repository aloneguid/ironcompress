using System.Buffers;
using System.Runtime.InteropServices;

namespace IronCompress
{
   /// <summary>
   /// Cross-platform P/Invoke wrapper as described in https://docs.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform
   /// </summary>
   public static class Iron
   {
      const string LibName = "nironcompress";

      [DllImport(LibName)]
      static extern unsafe bool compress(
         bool compress,
         int codec,
         byte* inputBuffer,
         int inputBufferSize,
         byte* outputBuffer,
         int* outputBufferSize);

      public static byte[]? Compress(
         Codec codec,
         ReadOnlySpan<byte> input,
         ArrayPool<byte> allocPool,
         out int outputSizeRet)
      {
         return CompressOrDecompress(true, codec, input, allocPool, out outputSizeRet);
      }

      public static byte[]? Decompress(
         Codec codec,
         ReadOnlySpan<byte> input,
         ArrayPool<byte> allocPool,
         out int outputSizeRet)
      {
         return CompressOrDecompress(false, codec, input, allocPool, out outputSizeRet);
      }


      /// <summary>
      /// Compress or decompress
      /// </summary>
      /// <param name="compressOrDecompress">When true, compresses, otherwise decompresses</param>
      /// <param name="codec">Compression codec</param>
      /// <param name="input">Input data</param>
      /// <param name="allocPool">Byte pool allocator</param>
      /// <param name="outputSizeRet">Size of result data. Due to usage of byte pool, the resulting buffer size may (and mostly will) be larger than result data size. You can convert this to Span and return data to pool later.</param>
      /// <returns></returns>
      public static byte[]? CompressOrDecompress(
         bool compressOrDecompress,
         Codec codec,
         ReadOnlySpan<byte> input,
         ArrayPool<byte> allocPool,
         out int outputSizeRet)
      {
         int outputSize = 0;

         unsafe
         {
            fixed (byte* inputPtr = input)
            {
               bool ok = compress(
                  compressOrDecompress,
                  (int)codec, inputPtr, input.Length, null, &outputSize);
               if (!ok)
               {
                  outputSizeRet = 0;
                  return null;
               }

               byte[]? output = allocPool.Rent(outputSize);
               //outputSize = output.Length;

               fixed (byte* outputPtr = output)
               {
                  ok = compress(
                     compressOrDecompress,
                     (int)codec, inputPtr, input.Length, outputPtr, &outputSize);

                  outputSizeRet = outputSize;
                  return output;
               }
            }
         }
      }
   }
}
