//type:fp
//options_all:--c++14
//remark:[4.13] C++-generating back end: Abort on member variable template declaration
// 12/8/16  [EDGcpfe/17620]
//
// C++-generating back end: Abort on member variable template declaration
//
// The C++-generating back end previously aborted when attempting to render a
// C++14 member variable template declaration lacking an in-class initializer.
//
// Although the abort occurred in the C++-generating back end (in function
// gen_template) it was due to the front end producing an unintended sequence of
// source sequence entries.  This is now fixed.
struct S {
  template<typename T> static T m;  // Previously triggered an internal
};                                  // error in cp_gen_be.c.
