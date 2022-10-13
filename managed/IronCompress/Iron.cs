using System.Buffers;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace IronCompress {
    public enum Codec {
        Snappy = 1,
        Zstd = 2,
        Gzip = 3,
        Brotli = 4,
        LZO = 5,
        LZ4 = 6
    }

    /// <summary>
    /// Cross-platform P/Invoke wrapper as described in https://docs.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform
    /// </summary>
    public class Iron {
        const string LibName = "nironcompress";
        private readonly ArrayPool<byte> _allocPool;

#if NET6_0_OR_GREATER
      private const CompressionLevel CL = CompressionLevel.SmallestSize;
#else
        private const CompressionLevel CL = CompressionLevel.Optimal;
#endif

        public Iron(ArrayPool<byte> allocPool = null) {
            _allocPool = allocPool;
        }

        [DllImport(LibName)]
        static extern unsafe bool compress(
           bool compress,
           int codec,
           byte* inputBuffer,
           int inputBufferSize,
           byte* outputBuffer,
           int* outputBufferSize);

        public DataBuffer Compress(
           Codec codec,
           ReadOnlySpan<byte> input,
           int? outputLength = null) {
            return CompressOrDecompress(true, codec, input, outputLength);
        }
        public DataBuffer Decompress(
           Codec codec,
           ReadOnlySpan<byte> input,
           int? outputLength = null) {
            return CompressOrDecompress(false, codec, input, outputLength);
        }


        /// <summary>
        /// Compress or decompress
        /// </summary>
        /// <param name="compressOrDecompress">When true, compresses, otherwise decompresses</param>
        /// <param name="codec">Compression codec</param>
        /// <param name="input">Input data</param>
        /// <returns></returns>
        public DataBuffer CompressOrDecompress(
           bool compressOrDecompress,
           Codec codec,
           ReadOnlySpan<byte> input,
           int? outputLength = null) {

            byte[] result;

            switch(codec) {
                case Codec.Gzip:
                    result = compressOrDecompress
                       ? Gzip(input)
                       : Ungzip(input);
                    return new DataBuffer(result, -1, null);


                case Codec.Brotli:
                    result = compressOrDecompress
                       ? BrotliCompress(input)
                       : BrotliUncompress(input);
                    return new DataBuffer(result, -1, null);
            }

            int len = 0;

            unsafe {
                fixed(byte* inputPtr = input) {
                    // get output buffer size into "len"
                    if(outputLength == null) {
                        bool ok = compress(
                           compressOrDecompress,
                           (int)codec, inputPtr, input.Length, null, &len);
                        if(!ok) {
                            throw new InvalidOperationException($"unable to detect result length");
                        }
                    }
                    else {
                        len = outputLength.Value;
                    }

                    byte[] output = _allocPool == null
                       ? new byte[len]
                       : _allocPool.Rent(len);

                    fixed(byte* outputPtr = output) {
                        try {
                            bool ok = compress(
                               compressOrDecompress,
                               (int)codec, inputPtr, input.Length, outputPtr, &len);

                            if(!ok) {
                                throw new InvalidOperationException($"compression failure");
                            }

                            return new DataBuffer(output, len, _allocPool);
                        }
                        catch {
                            if(_allocPool != null) {
                                _allocPool.Return(output);
                            }
                            throw;
                        }
                    }
                }
            }
        }

        private static byte[] Gzip(ReadOnlySpan<byte> data) {
            using(var compressedStream = new MemoryStream()) {
                using(var zipStream = new GZipStream(compressedStream, CL)) {
                    zipStream.Write(data);
                    zipStream.Flush();
                    zipStream.Close();
                    return compressedStream.ToArray();
                }
            }
        }

        private static byte[] BrotliCompress(ReadOnlySpan<byte> data) {
            using(var compressedStream = new MemoryStream()) {
                using(var zipStream = new BrotliStream(compressedStream, CL)) {
                    zipStream.Write(data);
                    zipStream.Flush();
                    zipStream.Close();
                    return compressedStream.ToArray();
                }
            }
        }

        private static byte[] Ungzip(ReadOnlySpan<byte> data) {
            using(var compressedStream = new MemoryStream()) {
                compressedStream.Write(data);
                compressedStream.Position = 0;
                using(var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress)) {
                    using(var resultStream = new MemoryStream()) {
                        zipStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }

        private static byte[] BrotliUncompress(ReadOnlySpan<byte> data) {
            using(var compressedStream = new MemoryStream()) {
                compressedStream.Write(data);
                compressedStream.Position = 0;
                using(var zipStream = new BrotliStream(compressedStream, CompressionMode.Decompress)) {
                    using(var resultStream = new MemoryStream()) {
                        zipStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }

        //
    }
}