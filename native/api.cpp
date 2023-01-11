#include "api.h"
#include "snappy.h"
#include "zstd.h"
#include "minilzo.h"
#include "lz4.h"
#include <string>
#include <vector>
#include "brotli/encode.h"
#include "brotli/decode.h"
//#include <iostream>

using namespace std;

bool lzo_initialised{ false };

bool compress_snappy(
   bool compress,
   char* input_buffer,
   int input_buffer_size,
   char* output_buffer,
   int* output_buffer_size)
{
   if (output_buffer == nullptr)
   {
      if (compress)
         *output_buffer_size = snappy::MaxCompressedLength(input_buffer_size);
      else
      {
         size_t length;
         if (snappy::GetUncompressedLength(input_buffer, input_buffer_size, &length))
         {
            *output_buffer_size = length;
         }
         else
         {
            *output_buffer_size = 0;
            return false;
         }
      }

      return true;
   }

   if (compress)
   {
      size_t compressed_length{ 0 };
      snappy::RawCompress(input_buffer, input_buffer_size, output_buffer, &compressed_length);
      *output_buffer_size = compressed_length;
   }
   else
   {
      snappy::RawUncompress(input_buffer, input_buffer_size, output_buffer);
   }

   return true;
}

bool compress_zstd(
    bool compress,
    char* input_buffer,
    int input_buffer_size,
    char* output_buffer,
    int* output_buffer_size) {
    if(output_buffer == nullptr) {
        if(compress) {
            *output_buffer_size = ZSTD_compressBound(input_buffer_size);
        } else {
            *output_buffer_size = ZSTD_getFrameContentSize(input_buffer, input_buffer_size);
        }
        return true;
    }

    if(compress) {
        *output_buffer_size = ZSTD_compress(output_buffer, *output_buffer_size, input_buffer, input_buffer_size, ZSTD_btultra2);
    } else {
        ZSTD_decompress(output_buffer, *output_buffer_size, input_buffer, input_buffer_size);
    }

    return true;
}

bool compress_brotli(
    bool compress,
    char* input_buffer,
    int input_buffer_size,
    char* output_buffer,
    int* output_buffer_size) {

    if(output_buffer == nullptr) {

        if(compress) {
            *output_buffer_size = ::BrotliEncoderMaxCompressedSize(input_buffer_size);
            return true;
        } else {
            return false;
        }
    }

    if(compress) {
        size_t compressed_size = *output_buffer_size;
        auto r = ::BrotliEncoderCompress(BROTLI_MAX_QUALITY, BROTLI_DEFAULT_WINDOW, BROTLI_DEFAULT_MODE,
            input_buffer_size, (const uint8_t*)input_buffer,
            &compressed_size, (uint8_t*)output_buffer);
        *output_buffer_size = compressed_size;
        return BROTLI_TRUE == r;
    } else {
        if(output_buffer_size == nullptr) return false;

        size_t decoded_size = *output_buffer_size;
        auto r = ::BrotliDecoderDecompress(input_buffer_size, (const uint8_t*)input_buffer,
            &decoded_size, (uint8_t*)output_buffer);
        *output_buffer_size = decoded_size;
        return BROTLI_DECODER_RESULT_SUCCESS == r;
    }
}

bool compress_lzo(
   bool compress,
   char* input_buffer,
   int input_buffer_size,
   char* output_buffer,
   int* output_buffer_size)
{
   //minilzo sample: https://github.com/nemequ/lzo/blob/master/minilzo/testmini.c

   if (!lzo_initialised)
   {
      //cout << "lzo init" << endl;

      if (lzo_init() == LZO_E_OK)
      {
         lzo_initialised = true;
      }
      else
      {
         return false;
      }
   }

   if (output_buffer == nullptr)
   {
      if (compress)
      {
         // We want to compress the data block at 'in' with length 'IN_LEN' to
         // the block at 'out'.Because the input block may be incompressible,
         // we must provide a little more output space in case that compression
         // is not possible.
         *output_buffer_size = input_buffer_size + input_buffer_size / 16 + 64 + 3;
      }
      else
      {
         // there is no way to estimate LZO buffer size, so we will just return immediately with false result.
         // One need to define their own framing format to store size, but that's not our aim.
         return false;
      }

      return true;
   }

   if (compress)
   {
      vector<char> wrkmem(LZO1X_1_MEM_COMPRESS);

      lzo_uint len{ 0 };
      int r = lzo1x_1_compress(
         (unsigned char*)input_buffer, input_buffer_size,
         (unsigned char*)output_buffer, &len,
         &wrkmem[0]);
      *output_buffer_size = len;

      return r == LZO_E_OK;
   }
   else
   {
      lzo_uint len{ 0 };
      int r = lzo1x_decompress(
         (unsigned char*)input_buffer, input_buffer_size,
         (unsigned char*)output_buffer, &len,
         nullptr);

      return r == LZO_E_OK;
   }

   return false;
}


bool compress_lz4(
    bool compress,
    char* input_buffer,
    int input_buffer_size,
    char* output_buffer,
    int* output_buffer_size) {
    if(output_buffer == nullptr) {
        if(compress) {
            *output_buffer_size = LZ4_compressBound(input_buffer_size);
            return true;
        } else {
            *output_buffer_size = 0;
            return false;
        }
    }

    if(compress) {
        *output_buffer_size = LZ4_compress_default(input_buffer, output_buffer, input_buffer_size, *output_buffer_size);
        return *output_buffer_size != 0;
    } else {
        *output_buffer_size = LZ4_decompress_safe(input_buffer, output_buffer, input_buffer_size, *output_buffer_size);
        return *output_buffer_size != 0;
    }

    return false;
}

bool compress(bool compress, int codec, char* input_buffer, int input_buffer_size,
    char* output_buffer, int* output_buffer_size) {
    switch(codec) {
        case 1:
            return compress_snappy(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
        case 2:
            return compress_zstd(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
            // 3 - gzip
        case 4:
            return compress_brotli(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
        case 5:
            return compress_lzo(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
        case 6:
            return compress_lz4(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
        default:
            return false;
    }
}