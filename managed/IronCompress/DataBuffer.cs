using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronCompress {
    /// <summary>
    /// Operation result with safe disposables.
    /// </summary>
    public class DataBuffer : IDisposable {
        private readonly byte[] _data;
        private readonly int _dataSize;
        private ArrayPool<byte> _arrayPool;

        /// <summary>
        /// Create an instance of this class.
        /// </summary>
        /// <param name="data">Data referenced by result.</param>
        /// <param name="dataSize">If data size is difference from input array size, pass the size explicitly.</param>
        /// <param name="arrayPool">When passed, will return to pool on dispose</param>
        public DataBuffer(byte[] data, int dataSize = -1, ArrayPool<byte> arrayPool = null) {
            _data = data;
            _dataSize = dataSize;
            _arrayPool = arrayPool;
        }

        public DataBuffer(byte[] data) : this(data, data.Length, null) {

        }

        public int Length => _dataSize == -1 ? _data.Length : _dataSize;

        public Span<byte> AsSpan() =>
           _data.AsSpan(0, Length);

        public static implicit operator Span<byte>(DataBuffer r) => r.AsSpan();

        public static implicit operator ReadOnlySpan<byte>(DataBuffer r) => r.AsSpan();

        public void Dispose() {
            if(_arrayPool == null)
                return;

            _arrayPool.Return(_data);
            _arrayPool = null;
        }
    }
}