#include "api.h"
#include "snappy.h"
#include "zstd.h"
#include "minilzo/minilzo.h"
#include <string>
#include <vector>

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
   int* output_buffer_size)
{
   if (output_buffer == nullptr)
   {
      if (compress)
      {
         *output_buffer_size = ZSTD_compressBound(input_buffer_size);
      }
      else
      {
         *output_buffer_size = ZSTD_getFrameContentSize(input_buffer, input_buffer_size);
      }
      return true;
   }

   if (compress)
   {
      *output_buffer_size = ZSTD_compress(output_buffer, *output_buffer_size, input_buffer, input_buffer_size, ZSTD_btultra2);
   }
   else
   {
      ZSTD_decompress(output_buffer, *output_buffer_size, input_buffer, input_buffer_size);
   }

   return true;
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

   return false;
}


bool compress(bool compress, int codec, char* input_buffer, int input_buffer_size,
   char* output_buffer, int* output_buffer_size)
{
   switch (codec)
   {
   case 1:
      return compress_snappy(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
   case 2:
      return compress_zstd(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
   case 3:
      return compress_lzo(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
   default:
      return false;
   }
}