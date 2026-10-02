//type:fp
//options_all:--ms_c++latest --microsoft_v 1914
//remark:[5.1] Abort in Microsoft mode on nonreal constexpr friend declaration
// 9/6/18   [EDGcpfe/19742,EDGcpfe/19982]
//
// Abort in Microsoft mode on nonreal constexpr friend declaration
//
// In Microsoft mode, the front end performs a "nonreal instantiation" of
// dependent base classes (to emulate certain nonstandard behaviors of the
// Microsoft compilers).  Previously, the front end could abort due to a failed
// assertion in check_use_of_constexpr when such a nonreal instantiation involved
// a constexpr friend function declaration.
//
// This is now fixed.
template<typename> struct B {
  friend constexpr bool f();
};
template <typename T> struct D: B<T> {};  // Previously an internal error in
                                          // Microsoft mode.  Now okay.
