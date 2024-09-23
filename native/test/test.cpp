#include "../lib/api.h"
#include <gtest/gtest.h>
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

TEST(Roundtrip, Snappy_1) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(1, bytes, 3));
}

TEST(Roundtrip, Zstd_2) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(2, bytes, 3));
}

TEST(Roundtrip, Brotli_4) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(4, bytes, 3));
}

TEST(Roundtrip, Lzo_5) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(5, bytes, 3));
}

TEST(Roundtrip, LZ4_6) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(6, bytes, 3));
}