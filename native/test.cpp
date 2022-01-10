#include "api.h"
#include <vector>

using namespace std;

bool run(int method, char* buffer, size_t buffer_length)
{
   int len{ 0 };
   compress(true, method, buffer, buffer_length, nullptr, &len);

   vector<char> compressed;
   compressed.resize(len);

   return compress(true, method, buffer, buffer_length, &compressed[0], &len);
}

int main()
{
   char bytes[] = {'a', 'b', 'c'};

   run(1, bytes, 3);
   run(2, bytes, 3);

   return 0;
}