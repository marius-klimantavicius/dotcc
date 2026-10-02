//type:fp
//options_all:--gnu=110200 --c++14
//remark:[6.4] __is_assignable with conversion operator template in source type
// 6/7/22   [EDGcpfe/25236]
//
// __is_assignable with conversion operator template in source type
//
// In cases where the __is_assignable type traits helper is invoked with a
// target type that cannot be assigned a value of the source type and the
// source type has a conversion operator template, the front end could produce
// an instance of that conversion operator template with an error type as the
// result type.  In configurations that mangle names, that could result in an
// assertion failure in record_substitution_in_type.
// --gnu_version=110200 --c++14:
//
// This previously resulted in an instance of D::operator T() where T was an
// error type.  This is now fixed.
template <int v> struct A {
  static constexpr bool val = v;
};
template<typename T> struct B : A<__is_constructible(T)> { };
template <typename T, typename U>
struct C : A<__is_assignable(T, U)> { };

template <bool> using enable_if_t = int;

struct D {
  template <typename T, typename = enable_if_t<B<T>::val>>
    operator T();
};
struct E {
  template <typename S, enable_if_t<C<int, S>::val> * = nullptr>
    void f(S);
};

D d;
E e;
void g() {
  e.f(d);
}
