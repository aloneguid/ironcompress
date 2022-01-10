#include "api.h"
#include "snappy.h"  // 1 - SNAPPY
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

bool compress(bool compress, int codec, char* input_buffer, int input_buffer_size,
   char* output_buffer, int* output_buffer_size)
{
   switch (codec)
   {
   case 1:
      compress_snappy(compress, input_buffer, input_buffer_size, output_buffer, output_buffer_size);
      break;
   default:
      return false;
   }

   return true;
}