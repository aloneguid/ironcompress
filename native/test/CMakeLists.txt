cmake_policy(SET CMP0091 NEW)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project ("test" VERSION 0.0)

find_package(Snappy CONFIG REQUIRED)
find_package(zstd CONFIG REQUIRED)
find_package(lz4 CONFIG REQUIRED)
find_package(unofficial-brotli CONFIG REQUIRED)
find_package(GTest CONFIG REQUIRED)

# For Windows: Prevent overriding the parent project's compiler/linker settings
set(gtest_force_shared_crt ON CACHE BOOL "" FORCE)

enable_testing()

add_executable(${PROJECT_NAME} "../lib/api.cpp" "../../minilzo/minilzo.c" "test.cpp")

include(GoogleTest)
gtest_discover_tests(${PROJECT_NAME})

target_link_libraries(${PROJECT_NAME} PRIVATE 
    Snappy::snappy
    zstd::libzstd_static
    lz4::lz4
    unofficial::brotli::brotlienc
    unofficial::brotli::brotlidec
    GTest::gtest GTest::gtest_main GTest::gmock GTest::gmock_main)

target_include_directories(${PROJECT_NAME} PRIVATE "../../minilzo")

# https://atomheartother.github.io/c++/2018/07/12/CPPDynLib.html

if(${CMAKE_CXX_COMPILER_ID} STREQUAL "MSVC")
    set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /MT")
    set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /MTd")
endif()