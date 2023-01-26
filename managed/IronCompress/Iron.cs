using System.Buffers;
using System.IO.Compression;

namespace IronCompress {

    /// <summary>
    /// Cross-platform P/Invoke wrapper as described in https://docs.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform
    /// </summary>
    public class Iron {

        private readonly ArrayPool<byte> _allocPool;


#if NET6_0_OR_GREATER
        private const CompressionLevel CL = CompressionLevel.SmallestSize;
#else
        private const CompressionLevel CL = CompressionLevel.Optimal;
#endif

        /// <summary>
        /// Create iron instance
        /// </summary>
        /// <param name="allocPool">Optionally specify array pool to use. When not set, will use <see cref="ArrayPool{byte}.Shared"/></param>
        public Iron(ArrayPool<byte> allocPool = null) {
            _allocPool = allocPool ?? ArrayPool<byte>.Shared;
        }

        public DataBuffer Compress(
           Codec codec,
           ReadOnlySpan<byte> input,
           int? outputLength = null,
           CompressionLevel? compressionLevel = null) {
            return CompressOrDecompress(true, codec, input, outputLength, compressionLevel);
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
        private DataBuffer CompressOrDecompress(
            bool compressOrDecompress,
            Codec codec,
            ReadOnlySpan<byte> input,
            int? outputLength = null,
            CompressionLevel? compressionLevel = null) {

            byte[] result;

            switch(codec) {
                case Codec.Gzip:
                    result = compressOrDecompress
                        ? Gzip(input, compressionLevel)
                        : Ungzip(input);
                    return new DataBuffer(result, -1, null);
            }

            int len = 0;
            int level = ConvertToNativeCompressionLevel(compressionLevel ?? CL);
            unsafe {
                fixed(byte* inputPtr = input) {
                    // get output buffer size into "len"
                    if(outputLength == null) {
                        bool ok = Native.compress(
                           compressOrDecompress,
                           (int)codec, inputPtr, input.Length, null, &len, level);
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
                            bool ok = Native.compress(
                                compressOrDecompress,
                                (int)codec, inputPtr, input.Length, outputPtr, &len, level);

                            if(!ok) {
                                throw new InvalidOperationException($"compression failure");
                            }

                            return new DataBuffer(output, len, _allocPool);
                        }
                        catch(System.AggregateException e) {
                            e.Handle(exception => exception is DllNotFoundException);

                            switch(codec) {
                                case Codec.Snappy:
                                        result = compressOrDecompress
                                            ? SnappyManagedCompress(input)
                                            : SnappyManagedUncompress(input);
                                        return new DataBuffer(result, -1, null);
#if !NETSTANDARD2_0
                                case Codec.Brotli:
                                    result = compressOrDecompress
                                        ? BrotliCompress(input, compressionLevel)
                                        : BrotliUncompress(input);
                                    return new DataBuffer(result, -1, null);
#endif
                            }
                            throw;
                        }
                        finally {
                            if(_allocPool != null) {
                                _allocPool.Return(output);
                            }
                        }
                    }
                }
            }
        }

        private int ConvertToNativeCompressionLevel(CompressionLevel compressionLevel) {
            switch(compressionLevel) {
                case CompressionLevel.NoCompression:
                case CompressionLevel.Fastest:
                    return 1;
                case CompressionLevel.Optimal:
                    return 2;
                default:
                    return 3;
            }
        }

        private static byte[] Gzip(ReadOnlySpan<byte> data, CompressionLevel? compressionLevel = null) {
            using(var compressedStream = new MemoryStream()) {
                using(var zipStream = new GZipStream(compressedStream, compressionLevel ?? CL)) {
#if NETSTANDARD2_0
                    byte[] tmp = data.ToArray();
                    zipStream.Write(tmp, 0, tmp.Length);
#else
                    zipStream.Write(data);
#endif
                    zipStream.Flush();
                    zipStream.Close();
                    return compressedStream.ToArray();
                }
            }
        }

#if !NETSTANDARD2_0
        private static byte[] BrotliCompress(ReadOnlySpan<byte> data, CompressionLevel? compressionLevel = null) {
            using(var compressedStream = new MemoryStream()) {
                using(var zipStream = new BrotliStream(compressedStream, compressionLevel ?? CL)) {
                    zipStream.Write(data);
                    zipStream.Flush();
                    zipStream.Close();
                    return compressedStream.ToArray();
                }
            }
        }
#endif

        private static byte[] SnappyManagedCompress(ReadOnlySpan<byte> data) {
            return Snappier.Snappy.CompressToArray(data);
        }

        private static byte[] Ungzip(ReadOnlySpan<byte> data) {
            using(var compressedStream = new MemoryStream()) {
#if NETSTANDARD2_0
                byte[] tmp = data.ToArray();
                compressedStream.Write(tmp, 0, tmp.Length);
#else
                compressedStream.Write(data);
#endif
                compressedStream.Position = 0;
                using(var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress)) {
                    using(var resultStream = new MemoryStream()) {
                        zipStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }

#if !NETSTANDARD2_0
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
#endif

        private static byte[] SnappyManagedUncompress(ReadOnlySpan<byte> data) {
            return Snappier.Snappy.DecompressToArray(data);
        }
    }
}