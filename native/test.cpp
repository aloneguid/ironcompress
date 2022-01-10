#include "api.h"
#include <vector>

using namespace std;

int main()
{
   char bytes[] = {'a', 'b', 'c'};

   int len{ 0 };
   compress(true, 1, bytes, 3, nullptr, &len);

   vector<char> compressed;
   compressed.resize(len);

   compress(true, 1, bytes, 3, &compressed[0], &len);

   return 0;
}