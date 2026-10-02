# Build Instructions

This document contains build instructions for building relatively-standard
debug and release builds for the most common operating systems via the official
build.

> [!TIP]
>
> The front end can be built simply by creating a preprocessor `#define` based
> configuration (to your taste/needs) in `src/defines.h` and then running
> something like `g++ -std=c++14 *.c` in the `src` directory.  The build
> documented here uses more tooling so that it can handle many different compilers,
> build tools, and configurations (in a single build system definition) without
> modifying anything in the source tree; it is by no means the _only_ way to
> build.

## Requirements

To build or develop the EDG front end you will need:

- [Python](https://www.python.org/) 3.6 or later
- [CMake](https://cmake.org/) 3.19 or later

and one of the following compilers (or later):

- g++:    5.2.0
- clang:  3.4
- MSVC:   VS 2017 (build 15.0)

Additionally, the default project build tool is
[ninja](https://ninja-build.org/); it is highly recommended but not strictly
required.  Consult the CMake
[generator](https://cmake.org/cmake/help/latest/manual/cmake-generators.7.html)
documentation for a list of additional supported build tools.

## Build Process

A number of CMake presets are available for building common variants of the EDG
front end.  While a full lists of presets can be viewed by executing
`cmake --list-presets` the most common options are listed below:

- "macos-arm-clang-debug"
- "macos-arm-clang-release"
- "linux-gcc-debug" (recommended) or "linux-clang-debug"
- "linux-gcc-release" (recommended) or "linux-clang-release"
- "windows-msvc-debug"
- "windows-msvc-release"

Once you've selected an appropriate preset, run
`cmake --preset '<PRESET NAME>'` (or if using a CMake build generator other
than ninja `cmake --preset '<PRESET NAME>' -G '<GENERATOR NAME>'`)
this will configure a front end build and leave a note about the build
directory.  As an example for `cmake --preset 'linux-gcc-debug'` this will
print:

```
...
-- Build files have been written to: <EDG PROJECT DIR>/build/gcc
```

simply enter that directory and run the `ninja` build tool (or your specified
build tool if you chose to use an alternative build generator).

