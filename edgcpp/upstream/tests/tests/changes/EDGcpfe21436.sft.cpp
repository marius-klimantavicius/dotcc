//type:fn
//options_all:--no_ms_permissive
//remark:[5.1] Microsoft compatibility: binding const volatile lvalue reference to rvalues
// 7/2/19   [EDGcpfe/21436]
//
// Microsoft compatibility: binding const volatile lvalue reference to rvalues
//
// In certain cases the Microsoft compiler allows binding const volatile lvalue
// references to rvalues.  This is not standard compliant, and when permissive
// mode is disabled, the Microsoft compiler no longer allows this.  The front end
// was already emulating the permissive mode (when microsoft_bugs is set), but was
// not emulating the non-permissive mode.
//
// This is now fixed.
using T = int;
T t{};
const volatile T& x =
  static_cast<T&&>(t); // Now rejected with --no_ms_permissive
