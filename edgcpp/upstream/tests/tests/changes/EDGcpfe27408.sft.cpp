//type:fp
//options_all:--ms_c++17
//remark:[6.7] Fold-expressions in Microsoft nonreal base instantiations
// 7/25/24  [EDGcpfe/27408]
//
// Fold-expressions in Microsoft nonreal base instantiations
//
// In Microsoft mode, the front end performs "nonreal instantiations" of dependent
// base class types (this is a process unique to Microsoft mode, used to better
// approximate some behaviors of MSVC).  Previously, fold-expressions occurring
// during that process resulted in spurious errors.
//
// Previously, that example triggered spurious errors in the attempted expansion
// of the fold-expression while instantiating B<T1, T2> with generic T1 and T2
// types.  That is now fixed.
template<typename T> struct S {};
template<typename... Ts> struct B {
  static constexpr bool F = ((S<Ts>::m) || ...);
};
template<class T1, class T2> struct D: B<T1, T2> {};
