//type:fn
//remark:[4.3] Pure-specifiers ("= 0") on member function definitions
// 12/15/10 [EDGcpfe/11220]
//
// Pure-specifiers ("= 0") on member function definitions
//
// The grammar in the C++ standard does not permit a member function declaration
// with a pure-specifier (i.e., "= 0" following the parameter list) to also be a
// function definition.  Version 4.2 unintentionally dropped that restriction.
// It has now been reinstated (a discretionary error is issued), except in
// Microsoft mode (where a warning is issued).
struct S {
  virtual void f() = 0 {}  // Accidentally accepted in version 4.2.
};                         // Now an error (except in Microsoft mode).
