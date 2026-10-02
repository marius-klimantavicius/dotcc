//type:fp
//options_all:--gnu=60100
//remark:[6.4] GNU/clang compatibility: default template arguments on class member templates
// 8/22/22  [EDGcpfe/25533]
//
// GNU/clang compatibility: default template arguments on class member templates
//
// Clang and, as of version 6.1, g++ allow adding default template arguments
// on an out-of-class definition of a member template of a non-template class.
// The front end has now been changed to accept this usage in the relevant
// emulation modes.  This also appears to be allowed by the current wording
// of the C++ Standard, so it is also accepted with --strict; however, there
// is some doubt as to whether that permission was intentional or accidental,
// so such default arguments are still diagnosed as an error in default mode,
// as well as Microsoft mode and for gnu_version < 60000.
// --gnu_version=60100:
struct S {
  template<typename> static int f();
};
template<typename=int>   // Default argument now accepted in some modes
int S::f() { return 0; }
