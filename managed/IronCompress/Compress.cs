using System.Buffers;
using System.Runtime.InteropServices;

namespace Iron
{
   /// <summary>
   /// Cross-platform P/Invoke wrapper as described in https://docs.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform
   /// </summary>
   static class Compress
   {
      const string LibName = "nironcompress";

      [DllImport(LibName)]
      static unsafe extern bool compress(
         bool compress,
         int codec,
         byte* inputBuffer,
         int inputBufferSize,
         byte* outputBuffer,
         int* outputBufferSize);

      public static byte[] Unipress(
         bool compressOrUncompress,
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
                  compressOrUncompress,
                  (int)codec, inputPtr, input.Length, null, &outputSize);
               if (!ok)
               {
                  outputSizeRet = 0;
                  return null;
               }

               byte[] output = allocPool.Rent(outputSize);
               //outputSize = output.Length;

               fixed (byte* outputPtr = output)
               {
                  ok = compress(
                     compressOrUncompress,
                     (int)codec, inputPtr, input.Length, outputPtr, &outputSize);

                  outputSizeRet = outputSize;
                  return output;
               }
            }
         }
      }
   }
}
