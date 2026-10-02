//type:fp
//options_all:--microsoft_v 1932 --c++17
//remark:[6.5] Microsoft C++ compatibility: overriding __declspec(nothrow) member functions
// 4/25/23  [EDGcpfe/25908]
//
// Microsoft C++ compatibility: overriding __declspec(nothrow) member functions
//
// In recent Microsoft C++17 modes, __declspec(nothrow) causes the member function
// to be treated as a noexcept function.  That in turn would ordinarily make
// overriding with a non-noexcept function an error (or, in some Microsoft C++
// modes, a warning).  However, it appears Microsoft compilers do not report such
// a mismatch and the front end now emulates that behavior.
struct B {
  virtual __declspec(nothrow) void f();
};
struct D: B {
  void f();  // Previously a warning or error.  Now silently accepted.
};
