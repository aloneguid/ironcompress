using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronCompress
{
   /// <summary>
   /// Operation result with safe disposables.
   /// </summary>
   public class Result : IDisposable
   {
      private readonly byte[] _data;
      private readonly int _dataSize;
      private ArrayPool<byte> _arrayPool;

      internal Result(byte[] data, int dataSize, ArrayPool<byte> arrayPool)
      {
         _data = data;
         _dataSize = dataSize;
         _arrayPool = arrayPool;
      }

      public Span<byte> AsSpan() =>
         _data.AsSpan(0, _dataSize == -1 ? _data.Length : _dataSize);

      public static implicit operator Span<byte>(Result r) => r.AsSpan();

      public static implicit operator ReadOnlySpan<byte>(Result r) => r.AsSpan();

      public void Dispose()
      {
         if (_arrayPool == null) return;

         _arrayPool.Return(_data);
         _arrayPool = null;
      }
   }
}
