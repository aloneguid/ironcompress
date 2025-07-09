#include "api.h"
#include <gtest/gtest.h>
#include <vector>

using namespace std;

std::string str1 = R"(
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus lacinia odio vitae vestibulum vestibulum. Cras venenatis euismod malesuada. Nullam ac erat ante. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla.

Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Curabitur sodales ligula in libero. Sed dignissim lacinia nunc. Curabitur tortor. Pellentesque nibh. Aenean quam. In scelerisque sem at dolor. Maecenas mattis. Sed convallis tristique sem. Proin ut ligula vel nunc egestas porttitor. Morbi lectus risus, iaculis vel, suscipit quis, luctus non, massa. Fusce ac turpis quis ligula lacinia aliquet. Mauris ipsum. Nulla metus metus, ullamcorper vel, tincidunt sed, euismod in, nibh. Quisque volutpat condimentum velit.

Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus lacinia odio vitae vestibulum vestibulum. Cras venenatis euismod malesuada. Nullam ac erat ante. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla.

Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Curabitur sodales ligula in libero. Sed dignissim lacinia nunc. Curabitur tortor. Pellentesque nibh. Aenean quam. In scelerisque sem at dolor. Maecenas mattis. Sed convallis tristique sem. Proin ut ligula vel nunc egestas porttitor. Morbi lectus risus, iaculis vel, suscipit quis, luctus non, massa. Fusce ac turpis quis ligula lacinia aliquet. Mauris ipsum. Nulla metus metus, ullamcorper vel, tincidunt sed, euismod in, nibh. Quisque volutpat condimentum velit.

Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus lacinia odio vitae vestibulum vestibulum. Cras venenatis euismod malesuada. Nullam ac erat ante. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla.

Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Curabitur sodales ligula in libero. Sed dignissim lacinia nunc. Curabitur tortor. Pellentesque nibh. Aenean quam. In scelerisque sem at dolor. Maecenas mattis. Sed convallis tristique sem. Proin ut ligula vel nunc egestas porttitor. Morbi lectus risus, iaculis vel, suscipit quis, luctus non, massa. Fusce ac turpis quis ligula lacinia aliquet. Mauris ipsum. Nulla metus metus, ullamcorper vel, tincidunt sed, euismod in, nibh. Quisque volutpat condimentum velit.
    )";

void test_roundtrip(compression_codec codec) {
    vector<char> uncompressed(str1.begin(), str1.end());
    int64_t compressed_length;

    // find out how much space we need for compression
    bool ok = iron_compress(true, codec,
        &uncompressed[0], uncompressed.size(), nullptr, &compressed_length, compression_level::best);
    EXPECT_TRUE(ok);

    // create buffer for compression
    vector<char> compressed;
    compressed.resize(compressed_length);
    
    // compress
    ok = iron_compress(true, codec,
        &uncompressed[0], uncompressed.size(),
        &compressed[0], &compressed_length, compression_level::best);
    EXPECT_TRUE(ok);

    // check that's it's compressed indeed
    EXPECT_LT(compressed_length, uncompressed.size());

    // decompress
    vector<char> decompressed1;
    decompressed1.resize(uncompressed.size());
    int64_t decompressed_length = uncompressed.size();
    ok = iron_compress(false, codec,
        &compressed[0], compressed_length,
        &decompressed1[0], &decompressed_length, compression_level::best);
    EXPECT_TRUE(ok);

    // check that decompressed is the same as original
    EXPECT_EQ(decompressed_length, uncompressed.size());
    EXPECT_EQ(decompressed1, uncompressed);
    
}


TEST(Roundtrip, Snappy) {
    test_roundtrip(compression_codec::snappy);
}

TEST(Roundtrip, Zstd) {
    test_roundtrip(compression_codec::zstd);
}

// no gzip

TEST(Roundtrip, Brotli) {
    test_roundtrip(compression_codec::brotli);
}

TEST(Roundtrip, Lzo) {
    test_roundtrip(compression_codec::lzo);
}

TEST(Roundtrip, Lz4) {
    test_roundtrip(compression_codec::lz4);
}

TEST(Infra, Ping) {
    EXPECT_TRUE(iron_ping());
}

TEST(Infra, IsSupported) {
    EXPECT_TRUE(iron_is_supported(compression_codec::snappy));
    EXPECT_TRUE(iron_is_supported(compression_codec::zstd));
    EXPECT_FALSE(iron_is_supported(compression_codec::gzip));
    EXPECT_TRUE(iron_is_supported(compression_codec::brotli));
    EXPECT_TRUE(iron_is_supported(compression_codec::lzo));
    EXPECT_TRUE(iron_is_supported(compression_codec::lz4));
}

TEST(Infra, Version) {
    const char* version = iron_version();
    EXPECT_TRUE(version != nullptr);
    EXPECT_TRUE(strlen(version) > 0);
}