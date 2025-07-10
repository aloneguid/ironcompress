using System.Buffers;
using System.IO.Compression;
using System.Runtime.InteropServices;
using ZstdSharp;

namespace IronCompress {

    /// <summary>
    /// Cross-platform P/Invoke wrapper as described in https://docs.microsoft.com/en-us/dotnet/standard/native-interop/cross-platform
    /// </summary>
    public class Iron {

        private readonly ArrayPool<byte> _allocPool;
        private static bool? _isNativeLibraryAvailable;

        /// <summary>
        /// Returns true if native compression library is available on this platform
        /// </summary>
        public static bool IsNativeLibraryAvailable {
            get {
                // cache the results, as it won't change during the lifetime of the app
                if(_isNativeLibraryAvailable == null) {
                    try {
                        Native.iron_ping();
                        _isNativeLibraryAvailable = true;

                    } catch(DllNotFoundException) {
                        _isNativeLibraryAvailable = false;
                    } catch(BadImageFormatException) { 
                        _isNativeLibraryAvailable = false;
                    }
                }

                return _isNativeLibraryAvailable ?? false;
            }
        }

        public static bool SupportsManaged(Codec c) {
#if NETSTANDARD2_0
            return c == Codec.Snappy || c == Codec.Gzip;
#else
            return c == Codec.Snappy || c == Codec.Gzip || c == Codec.Brotli || c == Codec.Zstd;
#endif
        }

        public static bool SupportsNative(Codec c) {
            if(!IsNativeLibraryAvailable) {
                return false;
            }
            return Native.iron_is_supported((int)c);
        }

        public static string? GetNativeVersion() {
            IntPtr ptr = Native.iron_version();
            string? version = Marshal.PtrToStringAnsi(ptr);
            return version;
        }

        /// <summary>
        /// Set to force specific platform. Used mostly in benchmarking tests, prefer not to set.
        /// </summary>
        public Platform? ForcePlatform { get; set; }

        /// <summary>
        /// Create iron instance
        /// </summary>
        /// <param name="allocPool">Optionally specify array pool to use. When not set, will use <see cref="ArrayPool{byte}.Shared"/></param>
        public Iron(ArrayPool<byte>? allocPool = null) {
            _allocPool = allocPool ?? ArrayPool<byte>.Shared;
        }

        public IronCompressResult Compress(
            Codec codec,
            ReadOnlySpan<byte> input,
            long? outputLength = null,
            CompressionLevel compressionLevel = CompressionLevel.Optimal) {

            if(ForcePlatform != null) { 
                if(ForcePlatform == Platform.Native) {
                    return NativeCompressOrDecompress(true, codec, input, compressionLevel, outputLength);
                } else if(ForcePlatform == Platform.Managed) {
                    return ManagedCompressOrDecompress(true, codec, input, compressionLevel, outputLength);
                }
            }

            if(SupportsNative(codec))
                return NativeCompressOrDecompress(true, codec, input, compressionLevel, outputLength);

            if(SupportsManaged(codec))
                return ManagedCompressOrDecompress(true, codec, input, compressionLevel, outputLength);

            throw CreateUnavailableException(codec);

        }

        public IronCompressResult Decompress(
           Codec codec,
           ReadOnlySpan<byte> input,
           long? outputLength = null) {

            if(ForcePlatform != null) {
                if(ForcePlatform == Platform.Native) {
                    return NativeCompressOrDecompress(false, codec, input, CompressionLevel.NoCompression, outputLength);
                } else if(ForcePlatform == Platform.Managed) {
                    return ManagedCompressOrDecompress(false, codec, input, CompressionLevel.NoCompression, outputLength);
                }
            }

            if(SupportsNative(codec))
                return NativeCompressOrDecompress(false, codec, input, CompressionLevel.NoCompression, outputLength);

            if(SupportsManaged(codec))
                return ManagedCompressOrDecompress(false, codec, input, CompressionLevel.NoCompression, outputLength);

            throw CreateUnavailableException(codec);
        }

        private static Exception CreateUnavailableException(Codec codec) {
#if NET6_0_OR_GREATER
            string ri = RuntimeInformation.RuntimeIdentifier;
#else
            string ri = "unknown";
#endif

            return new NotSupportedException(
                $"No compression codec for {codec} is available on this platform (arch: {RuntimeInformation.ProcessArchitecture}, rt: {ri}, native: {IsNativeLibraryAvailable})");

        }

        private IronCompressResult NativeCompressOrDecompress(
            bool compressOrDecompress,
            Codec codec,
            ReadOnlySpan<byte> input,
            CompressionLevel compressionLevel,
            long? outputLength = null) {

            long len = 0;
            int level = ToNativeCompressionLevel(compressionLevel);
            unsafe {
                fixed(byte* inputPtr = input) {
                    // get output buffer size into "len"
                    if(outputLength == null) {
                        bool ok = Native.iron_compress(
                           compressOrDecompress,
                           (int)codec, inputPtr, input.Length, null, &len, level);
                        if(!ok) {
                            throw new InvalidOperationException($"unable to detect result length");
                        }
                    } else {
                        len = outputLength.Value;
                    }

                    byte[] output = (_allocPool == null || len > int.MaxValue)
                       ? new byte[len]
                       : _allocPool.Rent((int)len);

                    fixed(byte* outputPtr = output) {
                        try {
                            bool ok = Native.iron_compress(
                               compressOrDecompress,
                               (int)codec, inputPtr, input.Length, outputPtr, &len, level);

                            if(!ok) {
                                throw new InvalidOperationException($"compression failure");
                            }

                            return new IronCompressResult(output, codec, true, len, _allocPool);
                        } catch {
                            if(_allocPool != null) {
                                _allocPool.Return(output);
                            }
                            throw;
                        }
                    }
                }
            }
        }

        private IronCompressResult ManagedCompressOrDecompress(
            bool compressOrDecompress,
            Codec codec,
            ReadOnlySpan<byte> input,
            CompressionLevel compressionLevel,
            long? outputLength = null) {

            byte[] result;

            switch(codec) {
                case Codec.Snappy:
                    result = compressOrDecompress
                        ? SnappyManagedCompress(input)
                        : SnappyManagedUncompress(input);
                    return new IronCompressResult(result, codec, false, -1, null);
                case Codec.Gzip:
                    result = compressOrDecompress
                       ? Gzip(input, compressionLevel)
                       : Ungzip(input);
                    return new IronCompressResult(result, codec, false, -1, null);

#if !NETSTANDARD2_0
                case Codec.Brotli:
                    result = compressOrDecompress
                        ? BrotliCompress(input, compressionLevel)
                        : BrotliUncompress(input);
                    return new IronCompressResult(result, codec, false, -1, null);
#endif

                case Codec.Zstd:
                    result = compressOrDecompress
                        ? ZstdManagedCompress(input, compressionLevel)
                        : ZstdManagedUncompress(input);
                    return new IronCompressResult(result, codec, false, -1, null);

                default:
                    throw new NotSupportedException($"managed compression for {codec} is not supported");
            }
        }

        private int ToNativeCompressionLevel(CompressionLevel compressionLevel) {
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

        private static byte[] Gzip(ReadOnlySpan<byte> data, CompressionLevel compressionLevel) {
            using(var compressedStream = new MemoryStream()) {
                using(var zipStream = new GZipStream(compressedStream, compressionLevel)) {
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

        private static byte[] ZstdManagedCompress(ReadOnlySpan<byte> data, CompressionLevel compressionLevel) {

            int level = compressionLevel switch {
                CompressionLevel.Optimal => 3,
                CompressionLevel.Fastest => 1,
                CompressionLevel.NoCompression => 1,
#if NET6_0_OR_GREATER
                CompressionLevel.SmallestSize => 19,
#endif
                _ => 0
            };

            using var compressor = new ZstdSharp.Compressor(level);
            return compressor.Wrap(data).ToArray();
        }

#if !NETSTANDARD2_0
        private static byte[] BrotliCompress(ReadOnlySpan<byte> data, CompressionLevel compressionLevel) {
            using(var compressedStream = new MemoryStream()) {
                using(var zipStream = new BrotliStream(compressedStream, compressionLevel)) {
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

        private static byte[] ZstdManagedUncompress(ReadOnlySpan<byte> data) {
            using var decompressor = new ZstdSharp.Decompressor();
            return decompressor.Unwrap(data).ToArray();
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