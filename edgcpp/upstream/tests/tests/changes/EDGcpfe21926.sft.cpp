//type:fp
//options_all:--c++11
//remark:[6.0] __INTADDR__ and constexpr evaluation
// 10/19/19 [EDGcpfe/21926]
//
// __INTADDR__ and constexpr evaluation
//
// The EDG-specific extension __INTADDR__ (which enables the folding of its
// operand even if that operand includes operations that require adding an offset
// to a null pointer) has been reworked to work with the constexpr interpreter.
// That enables some cases that had become errors in modes that support
// constexpr.
#define offsetof(T, member)  (__INTADDR__((&((T *)0)->member)))
struct S {
  unsigned arr[4];
};
enum { N = offsetof(S, arr[1]), };  // Previously an error in C++11 mode.
                                    // Now okay.
