#include "api.h"
#include <gtest/gtest.h>
#include <vector>

using namespace std;

bool run(compression_codec method, char* buffer, size_t buffer_length) {
    int32_t len{0};
    bool ok = iron_compress(true, method, buffer, buffer_length, nullptr, &len, compression_level::best);

    vector<char> compressed;
    compressed.resize(len);

    ok = iron_compress(true, method, buffer, buffer_length,
        &compressed[0], &len, compression_level::best);

    vector<byte> decompressed;
    int32_t bl1 = buffer_length;
    decompressed.resize(buffer_length);
    ok = iron_compress(false, method, &compressed[0], len,
       (char*)&decompressed[0], &bl1, compression_level::best);

    // todo: compare buffer sizes

    return ok;
}

TEST(Roundtrip, Snappy_1) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(compression_codec::snappy, bytes, 3));
}

TEST(Roundtrip, Zstd_2) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(compression_codec::zstd, bytes, 3));
}

TEST(Roundtrip, Gzip_3) {
    EXPECT_FALSE(iron_is_supported(compression_codec::gzip));
    EXPECT_FALSE(iron_compress(true, compression_codec::gzip,
        nullptr, 0,
        nullptr, nullptr,
        compression_level::best));

    //char bytes[] = {'a', 'b', 'c'};
    //EXPECT_TRUE(run(3, bytes, 3));
}

TEST(Roundtrip, Brotli_4) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(compression_codec::brotli, bytes, 3));
}

TEST(Roundtrip, Lzo_5) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(compression_codec::lzo, bytes, 3));
}

TEST(Roundtrip, LZ4_6) {
    char bytes[] = {'a', 'b', 'c'};
    EXPECT_TRUE(run(compression_codec::lz4, bytes, 3));
}