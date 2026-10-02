//type:fp
//options: -A --c++26

#include <utility>

template<typename>
void f()
{
  constexpr auto [...Is] = std::make_index_sequence<2>();
  static_assert(Is...[0] + Is...[1] == 1);
}

template void f<void>();

//cwg: 3135
//title: constexpr structured bindings with prvalues from tuples
//meeting: Croydon 3/26
