using System.Buffers;

namespace IronCompress {
    /// <summary>
    /// Operation result with safe disposables.
    /// </summary>
    public class IronCompressResult : IDisposable {
        private readonly byte[] _data;
        private readonly int _dataSize;
        private ArrayPool<byte>? _arrayPool;

        /// <summary>
        /// Create an instance of this class.
        /// </summary>
        /// <param name="data">Data referenced by result.</param>
        /// <param name="dataSize">If data size is difference from input array size, pass the size explicitly.</param>
        /// <param name="arrayPool">When passed, will return to pool on dispose</param>
        public IronCompressResult(byte[] data, Codec codec, bool nativeUsed, int dataSize = -1, ArrayPool<byte>? arrayPool = null) {
            _data = data;
            Codec = codec;
            NativeUsed = nativeUsed;
            _dataSize = dataSize;
            _arrayPool = arrayPool;
        }

        public int Length => _dataSize == -1 ? _data.Length : _dataSize;

        /// <summary>
        /// Compression codec used
        /// </summary>
        public Codec Codec { get; }

        /// <summary>
        /// Was native compression used or managed
        /// </summary>
        public bool NativeUsed { get; }

        public Span<byte> AsSpan() =>
           _data.AsSpan(0, Length);

        public static implicit operator Span<byte>(IronCompressResult r) => r.AsSpan();

        public static implicit operator ReadOnlySpan<byte>(IronCompressResult r) => r.AsSpan();

        public void Dispose() {
            if(_arrayPool == null)
                return;

            _arrayPool.Return(_data);
            _arrayPool = null;
        }
    }
}