//type:fp
//options_all:--c++17
//remark:[4.14] C++17 compatibility: capturing *this by value
// 5/15/17  [EDGcpfe/17825,EDGcpfe/17998]
//
// C++17 compatibility: capturing *this by value
//
// In modes enabling C++17 compatibility, the front end now accepts "*this" as
// a lambda capture, signifying that the entire object to which "this" points
// (and not just the value of the "this" pointer) is to be copied into the
// closure object (see Committee document P0018R3).  Support for this feature
// involves a small IL CHANGE: when the captured.variable field of
// a_lambda_capture designates a "this" parameter, the capture_by_reference
// flag is TRUE to indicate that the pointer value of "this" is captured and
// FALSE when the entire "*this" object is captured. (That flag was previously
// FALSE when "this" was captured.)
struct S {
  int i;
  void f() const {
    auto L = [*this]{ return i; };  // lambda captures the entire S object
  }
};
