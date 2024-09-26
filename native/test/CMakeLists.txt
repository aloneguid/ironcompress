cmake_policy(SET CMP0091 NEW)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project ("test" VERSION 0.0)

find_package(GTest CONFIG REQUIRED)

# For Windows: Prevent overriding the parent project's compiler/linker settings
set(gtest_force_shared_crt ON CACHE BOOL "" FORCE)

enable_testing()
add_executable(${PROJECT_NAME} "test.cpp")
target_link_libraries(${PROJECT_NAME} GTest::gtest_main nironcompress)

include(GoogleTest)
gtest_discover_tests(${PROJECT_NAME})

set_property(TARGET ${PROJECT_NAME} PROPERTY
  MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")