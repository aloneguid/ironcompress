#include "native-codecs.h"
#include "snappy.h"  // 1 - SNAPPY
#include <string>

using namespace std;

void encode_snappy(char* input_buffer,
   int input_buffer_size,
   char* output_buffer,
   int* output_buffer_size)
{
   if (output_buffer == nullptr)
   {
      *output_buffer_size = snappy::MaxCompressedLength(input_buffer_size);
      return;
   }

   size_t compressed_length{ 0 };
   snappy::RawCompress(input_buffer, input_buffer_size, output_buffer, &compressed_length);
   *output_buffer_size = compressed_length;
}

void decode_snappy(char* input_buffer,
   int input_buffer_size,
   char* output_buffer,
   int* output_buffer_size)
{
   if (output_buffer == nullptr)
   {
      size_t uncompressed_length{ 0 };
      snappy::GetUncompressedLength(input_buffer, input_buffer_size, &uncompressed_length);
      *output_buffer_size = uncompressed_length;
      return;
   }

   snappy::RawUncompress(input_buffer, input_buffer_size, output_buffer);

   // no need to change output_buffer_size as it's exactly as passed in
}

bool encode(int codec, char* input_buffer, int input_buffer_size,
   char* output_buffer, int* output_buffer_size)
{
   switch (codec)
   {
   case 1:
      encode_snappy(input_buffer, input_buffer_size, output_buffer, output_buffer_size);
      break;
   default:
      return false;
   }

   return true;
}

bool decode(int codec,
   char* input_buffer, int input_buffer_size,
   char* output_buffer, int* output_buffer_size)
{
   switch (codec)
   {
   case 1:
      decode_snappy(input_buffer, input_buffer_size, output_buffer, output_buffer_size);
      break;
   default:
      return false;
   }

   return true;
}