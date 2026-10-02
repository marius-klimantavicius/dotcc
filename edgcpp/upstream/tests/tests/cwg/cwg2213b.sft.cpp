//type:fn
//options_all:--c++20 -tused -A
namespace N {
  struct A;
  template<typename T> struct B {};
}
template<typename T> struct C {};
struct D {
  template<typename T> struct A {};
};
struct N::A; // #1

template<typename T> struct N::B; // #1
template<typename T> struct N::B<T*>; // #2
template<> struct N::B<int>;
template struct N::B<float>;

template<typename T> struct C;
template<typename T> struct C<T*>; // #2
template<> struct C<int>;
template struct C<float>;

template<typename T> struct D::A; // #1
template<typename T> struct D::A<T*>; // #2
template<> struct D::A<int>;
template struct D::A<float>;
