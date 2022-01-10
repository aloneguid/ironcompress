#include "native-codecs.h"
#include <vector>

using namespace std;

int main()
{
   char bytes[] = {'a', 'b', 'c'};

   int len{ 0 };
   encode(1, bytes, 3, nullptr, &len);

   vector<char> compressed;
   compressed.resize(len);

   encode(1, bytes, 3, &compressed[0], &len);

   return 0;
}