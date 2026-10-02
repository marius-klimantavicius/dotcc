//type:fp
//options:--c++23 -A

struct A {
  union { int x; };
  int y;
};
struct B : A {};

template <typename> struct Q;
template <> struct Q<A> {};
template <typename T> Q<T> f(int T::*);

Q<A> g() { return f(&B::x); }  // OK
Q<A> h() { return f(&B::y); }  // OK

//cwg: 3026
//title: Class for pointer-to-member formation
//meeting: Sofia 6/25
//edg_status: Passes
