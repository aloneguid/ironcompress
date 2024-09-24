#pragma once

#include <stdint.h>

#ifdef _WIN32
# ifdef WIN_EXPORT
#   define EXPORTED  __declspec( dllexport )
# else
#   define EXPORTED  __declspec( dllimport )
# endif
#else
# define EXPORTED
#endif

enum class compression_level : int32_t {
    fastest = 1,
    balanced = 2,
    best = 3
};

enum class compression_codec : int32_t {
    snappy = 1,
    zstd = 2,
    gzip = 3,
    brotli = 4,
    lzo = 5,
    lz4 = 6
};

extern "C"
{
   /**
    * @brief Encode (compress) or decompress
    * @param compress When true this is compression, otherwise decompression.
    * @param codec 1 - snappy, 2 - zstd, 3 - gzip, 4 - brotli, 5 - lzo, 6 - lz4
    * @param input_buffer If this is set to nullptr, the function sets output_buffer_size to required maximum size of the compressed data.
    * @param input_buffer_size 
    * @param output_buffer When output_buffer is nullptr, this is set to maximum buffer size required. Otherwise, to the size of the actual compressed data written to output_buffer.
    * @param output_buffer_size 
    * @param compression_level 1 - fastest, 2 - balanced, 3 - best
    * @return 
   */
   EXPORTED bool iron_compress(
      bool compress,
      int32_t codec,
      char* input_buffer,
      int32_t input_buffer_size,
      char* output_buffer,
      int32_t* output_buffer_size,
      compression_level compression_level);

   /**
    * @brief Used to just ping the library to test it's available at all
    */
   EXPORTED bool iron_ping();
}
