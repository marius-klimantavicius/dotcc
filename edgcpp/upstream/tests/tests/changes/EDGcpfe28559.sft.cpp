//type:fp
//options_all:--g++ --c++11
//remark:GNU compatibility: __cpp_init_captures in C++11 mode
// 11/25/25 [EDGcpfe/28559]
//
// GNU compatibility: __cpp_init_captures in C++11 mode
//
// Although g++ supports lambda init-captures with -std=c++11, it does not
// define the corresponding feature-test macro because they were part of C++14
// (see EDGcpfe/28556).  The front end previously did not make a similar
// distinction and defined __cpp_init_captures whenever the feature was
// supported in the current emulation.  This is now fixed.
// --g++ --c++11:
//
// -------------------------------------------------------------------------------
#ifdef __cpp_init_captures
#error Incorrect feature-test macro
#endif
