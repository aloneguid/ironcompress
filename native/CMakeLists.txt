cmake_minimum_required (VERSION 3.15)
cmake_policy(SET CMP0091 NEW)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project ("ironcompress" VERSION 0.0)

find_package(Snappy CONFIG REQUIRED)
find_package(zstd CONFIG REQUIRED)
#find_package(lzokay CONFIG REQUIRED)

add_library (nironcompress SHARED "api.cpp")
set_property(TARGET nironcompress PROPERTY
  MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

add_executable(tester "api.cpp" "test.cpp")
set_property(TARGET tester PROPERTY
  MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")


if(${CMAKE_CXX_COMPILER_ID} STREQUAL "MSVC")
   # MSVC
    target_compile_options(nironcompress PRIVATE /EHsc /MTd /W2 /c)
    # Set the DLLEXPORT variable to export symbols
    target_compile_definitions(nironcompress PRIVATE WIN_EXPORT)
endif()

target_link_libraries(nironcompress PRIVATE Snappy::snappy zstd::libzstd_static)
target_link_libraries(tester PRIVATE Snappy::snappy zstd::libzstd_static)
#target_link_libraries(main PRIVATE lzokay::lzokay)

# https://atomheartother.github.io/c++/2018/07/12/CPPDynLib.html

if(${CMAKE_CXX_COMPILER_ID} STREQUAL "MSVC")
    set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /MT")
    set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /MTd")
endif()

message(INFO " binary dir:" ${CMAKE_BINARY_DIR})

add_custom_command(
    TARGET nironcompress 
    POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy
        $<TARGET_FILE:nironcompress>
        ${CMAKE_SOURCE_DIR}/bin/$<TARGET_FILE_NAME:nironcompress>
)