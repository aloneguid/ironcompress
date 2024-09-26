cmake_minimum_required (VERSION 3.20)
cmake_policy(SET CMP0091 NEW)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project ("ironcompress" VERSION 0.0)

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

add_subdirectory(lib)
# only build tests if SUPPRESS_TEST environment variable is not set
if(NOT DEFINED ENV{SUPPRESS_TEST})
    add_subdirectory(test)
endif()