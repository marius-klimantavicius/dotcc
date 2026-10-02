//type:fp
//options_all:--c++17
//remark:[5.1] Incorrect resolution of tuple case in structured bindings
// 4/9/19   [EDGcpfe/21063]
//
// Incorrect resolution of tuple case in structured bindings
//
// If a std::tuple_size has been provided that is not well-formed when
// instantiating it with std::tuple_size<E>, the front end would previously
// accept that instance anyways.  This would lead to erroneously using the tuple
// case for structured bindings at best, and an abort at worst.
//
// Now fixed.
struct A { int i; };
namespace std {
  template<typename T, typename U>
  struct tuple_size { static constexpr int value = 1; };
}
void f() {
  A a{42};
  auto [ai] = a; // Would previously attempt to instantiate
                 // std::tuple_size<A> and use the tuple case to resolve the
                 // declaration.
}
