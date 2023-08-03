#include "api.h"
#include <vector>

using namespace std;

bool run(int method, char* buffer, size_t buffer_length) {
    int32_t len{0};
    bool ok = compress(true, method, buffer, buffer_length, nullptr, &len, compression_level::best);

    vector<char> compressed;
    compressed.resize(len);

    ok = compress(true, method, buffer, buffer_length,
        &compressed[0], &len, compression_level::best);

    vector<byte> decompressed;
    int32_t bl1 = buffer_length;
    decompressed.resize(buffer_length);
    ok = compress(false, method, &compressed[0], len,
       (char*)&decompressed[0], &bl1, compression_level::best);

    return ok;
}

int main() {
    char bytes[] = {'a', 'b', 'c'};

    //run(1, bytes, 3);
    //run(2, bytes, 3);
    run(6, bytes, 3);

    return 0;
}