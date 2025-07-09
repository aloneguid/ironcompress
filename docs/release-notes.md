## 1.6.4

- Add ` /guard:cf` by @johnthcall in #26.
- Increase buffer size variable resolution to `int64_t` (#28).

## 1.6.3

- Native build for Linux [musl](https://wiki.musl-libc.org/projects-using-musl.html) runtime.
- Fixed regression - when native library cannot be loaded, the entire compression fails.

## 1.6.2

- Improvement: native library can return it's version.
- Bug fixed: marshalling `bool` return type from C++ is different from C, which resulted in wrong error code passed back to C#.

## 1.6.1

### Improvements

- internal: added native code unit tests with Google Test (+stability)

## 1.6.0

Added new **native** compression method (zlib).