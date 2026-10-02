//type:fp
//options_all:--microsoft_v 1929
//remark:[6.3] Microsoft compatibility: __restrict parameters
// 10/25/21 [EDGcpfe/24520]
//
// Microsoft compatibility: __restrict parameters
//
// The front end previously ignored top-level __restrict qualifiers on parameter
// types when comparing function types.  Now, such qualifiers matter in Microsoft
// modes.
//
// Furthermore, a new command-line option --keep_restrict_in_signatures causes the
// front end to keep the restrict qualifier in a_param_type::type even when
// remove_qualifiers_from_param_types is TRUE (this causes the restrict qualifier
// to be incorporated in the mangled name, which matches the behavior of the
// Microsoft compiler).  A --no_keep_restrict_in_signatures option is also
// available.
template<typename, typename>
  struct is_same { static const bool value = false; };
template<typename T>
  struct is_same<T,T> { static const bool value = true; };
using type = float *__restrict;
void g(type) {
  static_assert(!is_same<decltype(g)*, void (*)(float*)>::value, "");
    // Now accepted in Microsoft C++ modes.
}
