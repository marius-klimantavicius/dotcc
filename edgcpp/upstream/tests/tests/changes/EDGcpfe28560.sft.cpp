//type:fp
//options_all:--c++11 --g++
//remark:GNU compatibility: __cpp_runtime_arrays
// 11/25/25 [EDGcpfe/28560]
//
// GNU compatibility: __cpp_runtime_arrays
//
// Although the feature is not part of standard C++, g++ supports
// variable-length arrays in C++ mode and sets the feature-test macro
// __cpp_runtime_arrays to the value 198712L.  The front end emulates the
// support for the feature, but it failed to set the feature-test macro.  This
// is now fixed.
#if !defined(__cpp_runtime_arrays) || __cpp_runtime_arrays != 198712
#error Missing macro.
#endif
