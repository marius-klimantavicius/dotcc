//type:fp
//options_all:--no_exceptions --c++11
//remark:[4.6] Abort in C++11 mode on defaulted special member when exceptions are disabled
// 11/13/12 [EDGcpfe/13363]
//
// Abort in C++11 mode on defaulted special member when exceptions are disabled
//
// Version 4.5 of the front end aborted with an internal error in function
// form_exception_specification_for_generated_function when defining a special
// member function with "= default" inside a class definition while exceptions
// are disabled (e.g., because of the --no_exceptions option).
//
// This is now fixed.  (This issue was a regression introduced by the changes
// for EDGcpfe/12860.)
struct S {
  S() = default;  // This triggered an abort in version 4.5 when the
};                // option "--no_exceptions" is used.
