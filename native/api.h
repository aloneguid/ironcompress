#pragma once

#ifdef _WIN32
# ifdef WIN_EXPORT
#   define EXPORTED  __declspec( dllexport )
# else
#   define EXPORTED  __declspec( dllimport )
# endif
#else
# define EXPORTED
#endif

extern "C"
{
   /// <summary>
   /// Encode (compress) or decompress
   /// </summary>
   /// <param name="compress">When true this is compression, otherwise decompression.</param>
   /// <param name="codec">Codec:
   /// 1 - snappy
   /// 2 - zstd
   /// 5 - lzo - in progress
   /// 4 - lz4 - todo
   /// </param>
   /// <param name="input_buffer"></param>
   /// <param name="input_buffer_size"></param>
   /// <param name="output_buffer">If this is set to nullptr, the function sets output_buffer_size to required maximum size of the compressed data.</param>
   /// <param name="output_buffer_size">When output_buffer is nullptr, this is set to maximum buffer size required. Otherwise, to the size of the actual compressed data written to output_buffer.</param>
   /// <returns></returns>
   EXPORTED bool compress(
      bool compress,
      int codec,
      char* input_buffer,
      int input_buffer_size,
      char* output_buffer,
      int* output_buffer_size); //
}
