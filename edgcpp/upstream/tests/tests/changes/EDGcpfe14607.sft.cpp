//type:fp
//options_all:--microsoft
//remark:[4.9] Microsoft mode unbounded recursion on elided copy constructor
// 11/1/13  [EDGcpfe/14607]
//
// Microsoft mode unbounded recursion on elided copy constructor
//
// The changes for EDGcpfe/14150 introduced a regression in Microsoft bugs mode
// that caused the front to abort due to unbounded recursion in some cases
// involving an elided copy constructor and classes that convert to each other.
//
// This is now fixed.  (See also EDGcpfe/10058, which addressed this same issue
// in all C++ modes.)
struct A;
struct B {
  B(A const&);
  B(B&);
};
struct A {
  A(B);
};
B f();
B g() {
  return f();  // Triggered unbounded recursion (and eventual abort) in
}              // Microsoft bugs mode.
