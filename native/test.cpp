#include "api.h"
#include <vector>

using namespace std;

bool run(int method, char* buffer, size_t buffer_length)
{
   int len{ 0 };
   bool ok = compress(true, method, buffer, buffer_length, nullptr, &len, 2);

   vector<char> compressed;
   compressed.resize(len);

   ok = compress(true, method, buffer, buffer_length, 
       &compressed[0], &len, 2);

   vector<byte> decompressed;
   int bl1 = buffer_length;
   decompressed.resize(buffer_length);
   ok = compress(false, method, &compressed[0], len,
      (char*)&decompressed[0], &bl1, 2);

   return ok;
}

int main()
{
   char bytes[] = {'a', 'b', 'c'};

   //run(1, bytes, 3);
   //run(2, bytes, 3);
   run(4, bytes, 3);

   return 0;
}