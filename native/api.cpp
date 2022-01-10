#include "api.h"
#include "snappy.h"
#include "zstd.h"
#include <string>

using namespace std;

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

bool compress(bool compress, int codec, char* input_buffer, int input_buffer_size,
   char* output_buffer, int* output_buffer_size)
{
   switch (codec)
   {
   case 1:
      return compress_snappy(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
   case 2:
      return compress_zstd(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
   default:
      return false;
   }
}