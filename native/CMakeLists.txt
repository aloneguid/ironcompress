cmake_minimum_required (VERSION 3.15)
cmake_policy(SET CMP0091 NEW)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project ("native-codecs" VERSION 0.0)

find_package(Snappy CONFIG REQUIRED)

add_library (codecs SHARED "native-codecs.cpp")
set_property(TARGET codecs PROPERTY
  MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

add_executable(tester "native-codecs.cpp" "test.cpp")
set_property(TARGET tester PROPERTY
  MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")


if(${CMAKE_CXX_COMPILER_ID} STREQUAL "MSVC")
   # MSVC
    target_compile_options(codecs PRIVATE /EHsc /MTd /W2 /c)
    # Set the DLLEXPORT variable to export symbols
    target_compile_definitions(codecs PRIVATE WIN_EXPORT)
endif()

target_link_libraries(codecs PRIVATE Snappy::snappy)
target_link_libraries(tester PRIVATE Snappy::snappy)

# https://atomheartother.github.io/c++/2018/07/12/CPPDynLib.html

if(${CMAKE_CXX_COMPILER_ID} STREQUAL "MSVC")
    set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /MT")
    set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /MTd")
endif()