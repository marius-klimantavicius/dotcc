//type:fp
//options_all:--c++11
//remark:[6.7] Incorrect expansion of sizeof... expression
// 11/6/24  [EDGcpfe/27428]
//
// Incorrect expansion of sizeof... expression
//
// When a sizeof... expression gets expanded with non-pack elements in addition to
// a pack, the substituted expression previously did not include the non-pack
// elements.
template<bool B>
struct C {
  static_assert(B, "Unexpected");  // Previously failed.  Now okay.
  using type = int;
};
template<typename... Us>
using A = C<sizeof...(Us) != 0>;
template<typename... Ts>
typename A<int, Ts...>::type f();
int i = f<>();
