cmake_minimum_required (VERSION 3.20)
cmake_policy(SET CMP0091 NEW)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project ("nironcompress" VERSION 0.0)

# find required packages
if(NOT "$ENV{NO_NATIVE_SNAPPY}" STREQUAL "true")
    find_package(Snappy CONFIG REQUIRED)
endif()
if(NOT "$ENV{NO_NATIVE_ZSTD}" STREQUAL "true")
    find_package(zstd CONFIG REQUIRED)
endif()
find_package(lz4 CONFIG REQUIRED)
find_package(unofficial-brotli CONFIG REQUIRED)
find_package(ZLIB REQUIRED)

# The cool "edit & continue" feature of MSVC is disabled by default in CMake.
# https://learn.microsoft.com/en-us/cpp/build/cmake-projects-in-visual-studio?view=msvc-170#edit-and-continue-for-cmake-projects
# enable globally: https://stackoverflow.com/a/76254117/80858
if(MSVC AND CMAKE_BUILD_TYPE MATCHES Debug)
    add_compile_options("/ZI")
    add_link_options("/INCREMENTAL")
endif()

if(MSVC AND CMAKE_BUILD_TYPE MATCHES Release)
    add_link_options("/OPT:REF")
endif()

# ------- Dynamic Library -----------

add_library (${PROJECT_NAME} SHARED "api.cpp" "../minilzo/minilzo.c")
target_compile_definitions(${PROJECT_NAME} PRIVATE
    IRON_VERSION="$ENV{VERSION}")
set_property(TARGET ${PROJECT_NAME} PROPERTY
  MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

if(${CMAKE_CXX_COMPILER_ID} STREQUAL "MSVC")
   # MSVC
    target_compile_options(${PROJECT_NAME} PRIVATE /EHsc /MTd /W2 /c)
    # Set the DLLEXPORT variable to export symbols
    target_compile_definitions(${PROJECT_NAME} PRIVATE WIN_EXPORT)
endif()

target_link_libraries(${PROJECT_NAME} PRIVATE 
    lz4::lz4
    unofficial::brotli::brotlienc
    unofficial::brotli::brotlidec
    ZLIB::ZLIB)


if("$ENV{NO_NATIVE_SNAPPY}" STREQUAL "true")
    add_compile_definitions(NO_NATIVE_SNAPPY)
else()
    target_link_libraries(${PROJECT_NAME} PRIVATE Snappy::snappy)
endif()

if("$ENV{NO_NATIVE_ZSTD}" STREQUAL "true")
    add_compile_definitions(NO_NATIVE_ZSTD)
else()
    target_link_libraries(${PROJECT_NAME} PRIVATE zstd::libzstd_static)
endif()

target_include_directories(${PROJECT_NAME} PRIVATE "../minilzo")

# https://atomheartother.github.io/c++/2018/07/12/CPPDynLib.html

if(${CMAKE_CXX_COMPILER_ID} STREQUAL "MSVC")
    set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /MT")
    set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /MTd")
endif()

# https://cmake.org/cmake/help/latest/manual/cmake-generator-expressions.7.html#generator-expression-reference
add_custom_command(
    TARGET ${PROJECT_NAME} 
    POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy
        $<TARGET_FILE:${PROJECT_NAME}>
        ${CMAKE_SOURCE_DIR}/ubin/runtimes/$ENV{DOTNET_RID}/native/$<TARGET_FILE_NAME:${PROJECT_NAME}>
)

# -------- Unit Tests ------------

find_package(GTest CONFIG REQUIRED)

# For Windows: Prevent overriding the parent project's compiler/linker settings
set(gtest_force_shared_crt ON CACHE BOOL "" FORCE)

enable_testing()
set(TEST_PROJECT_NAME "tests")
add_executable(${TEST_PROJECT_NAME} "test.cpp")
target_link_libraries(${TEST_PROJECT_NAME} GTest::gtest_main nironcompress)

include(GoogleTest)
gtest_discover_tests(${TEST_PROJECT_NAME} DISCOVERY_MODE PRE_TEST)

set_property(TARGET ${TEST_PROJECT_NAME} PROPERTY
  MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")