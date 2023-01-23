using System.Buffers;
using System.IO.Compression;

namespace IronCompress {

    /// <summary>
    /// Cross-platform P/Invoke wrapper as described in https://docs.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform
    /// </summary>
    public class Iron {

        private readonly ArrayPool<byte> _allocPool;
        private bool _useNativeBrotli;

        /// <summary>
        /// When set, native Brotli compressor is used instead of the managed implementation.
        /// Note that .NET Standard 2.0 does not have Brotli in .NET, so native compression is always used.
        /// </summary>
        public bool UseNativeBrotli {
            get { return _useNativeBrotli; }
            set {
#if NETSTANDARD2_0
                _useNativeBrotli = true;
#else
                _useNativeBrotli = value;
#endif
            }
        }

        /// <summary>
        /// When set, will use managed Snappy implementation. ON by default.
        /// </summary>
        public bool PreferManagedSnappy { get; set; } = true;


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
        private DataBuffer CompressOrDecompress(
            bool compressOrDecompress,
            Codec codec,
            ReadOnlySpan<byte> input,
            int? outputLength = null) {

            byte[] result;

            switch(codec) {
                case Codec.Snappy:
                    if(PreferManagedSnappy) {
                        result = compressOrDecompress
                            ? SnappyManagedCompress(input)
                            : SnappyManagedUncompress(input);
                        return new DataBuffer(result, -1, null);
                    }
                    break;
                case Codec.Gzip:
                    result = compressOrDecompress
                       ? Gzip(input)
                       : Ungzip(input);
                    return new DataBuffer(result, -1, null);

#if !NETSTANDARD2_0
                case Codec.Brotli:
                    if(!UseNativeBrotli) {
                        result = compressOrDecompress
                           ? BrotliCompress(input)
                           : BrotliUncompress(input);
                        return new DataBuffer(result, -1, null);
                    }
                    break;
#endif
            }

            int len = 0;

            unsafe {
                fixed(byte* inputPtr = input) {
                    // get output buffer size into "len"
                    if(outputLength == null) {
                        bool ok = Native.compress(
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
                            bool ok = Native.compress(
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