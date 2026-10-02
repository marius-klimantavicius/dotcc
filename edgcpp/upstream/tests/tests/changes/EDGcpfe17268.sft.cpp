//type:fp
//options_all:--c++11
//remark:[4.12] Assertion failure in constant_value_at_address
// 6/6/16   [EDGcpfe/17268,EDGcpfe/16646,EDGcpfe/16589]
//
// Assertion failure in constant_value_at_address
//
// The lowering process had neglected to properly set the
// size_without_virtual_base_classes field of the subobject type of a class,
// causing an assertion failure in constant_value_at_address in some
// configurations.
template<typename _Head> struct _Head_base {
  static constexpr const _Head&
  _M_head(const _Head_base& __b) { return __b._M_head_impl; }
  _Head _M_head_impl;
};
template<typename... _Elements> struct _Tuple_impl;
template<typename _Head, typename... _Tail>
struct _Tuple_impl<_Head, _Tail...>
  : public _Tuple_impl<_Tail...>, public _Head_base<_Head> {};
template<typename _Head>
struct _Tuple_impl<_Head> : public _Head_base<_Head> {};
class tuple : public _Tuple_impl<float, double> {};
constexpr bool foo(const tuple& t) {
  return _Head_base<float>::_M_head(t);
}
void f() {
  foo(tuple());
}
