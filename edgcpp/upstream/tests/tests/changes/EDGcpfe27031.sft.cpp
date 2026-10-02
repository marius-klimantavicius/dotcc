//type:fp
//options_all:--gn 120300 --c++17 --display_error
//remark:[6.7] GNU-mode abort on invalid call in template
// 2/21/24  [EDGcpfe/27031]
//
// GNU-mode abort on invalid call in template
//
// The call pmf(1) is invalid, but the front accepts the syntax in GNU-mode
// templates.  However, it previously aborted due to an error attempting to
// constant-evaluate the erroneous call.  That is now fixed.
template<typename... Ts> struct S {
  template<typename C> S(C *obj, void (C::*pmf)(Ts ...ps)) {
    pmf(1);
  }
};
