//type:fp
//options_all:--microsoft --c++14
//remark:[6.3] Microsoft C++ compatibility: Exception specifications
// 8/26/21  [EDGcpfe/15587,EDGcpfe/23976]
//
// Microsoft C++ compatibility: Exception specifications
//
// In Microsoft C++ mode, exception specifications are often ignored by the front
// end.  Previously, that process was a little too aggressive: A few tweaks have
// been made to more closely emulate the Microsoft compiler.
struct S {
  ~S() throw(int);
};
static_assert(!noexcept(S{}), "Error");  // Previously failed in Microsoft
                                         // modes.  Now okay.
