cmake_policy(SET CMP0091 NEW)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project ("nironcompress" VERSION 0.0)

find_package(Snappy CONFIG REQUIRED)
find_package(zstd CONFIG REQUIRED)
find_package(lz4 CONFIG REQUIRED)
find_package(unofficial-brotli CONFIG REQUIRED)

#find_package(PkgConfig)
#pkg_check_modules(zlib-ng REQUIRED IMPORTED_TARGET GLOBAL zlib-ng>=2.0)
find_package(zlib-ng CONFIG REQUIRED)

add_library (${PROJECT_NAME} SHARED "api.cpp" "../../minilzo/minilzo.c")
set_property(TARGET ${PROJECT_NAME} PROPERTY
  MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

if(${CMAKE_CXX_COMPILER_ID} STREQUAL "MSVC")
   # MSVC
    target_compile_options(${PROJECT_NAME} PRIVATE /EHsc /MTd /W2 /c)
    # Set the DLLEXPORT variable to export symbols
    target_compile_definitions(${PROJECT_NAME} PRIVATE WIN_EXPORT)
endif()

target_link_libraries(${PROJECT_NAME} PRIVATE 
    Snappy::snappy
    zstd::libzstd_static
    lz4::lz4
    unofficial::brotli::brotlienc
    unofficial::brotli::brotlidec)

target_include_directories(${PROJECT_NAME} PRIVATE "../../minilzo")

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